using System.Collections.Generic;
using PDSim.Protobuf;
using PDSim.Simulation;
using PDSim.Utils;
using PDSim.VisualScripting;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Editor.UI
{
    /// <summary>
    /// Custom editor window for creating new animations.
    /// </summary>
    public class CreateAnimationWindow : EditorWindow
    {
        public VisualTreeAsset predicateAnimationAttributeTemplate;

        private PdSimFluent _metadata;

        private ScrollView _predicateAnimationAttributeList;

        private FluentAnimation _context;

        public static void ShowAsModal(PdSimFluent metadata, FluentAnimation context)
        {
            var wnd = GetWindow<CreateAnimationWindow>();
            wnd.titleContent = new GUIContent("Create New Animation");
            wnd._metadata = metadata;
            wnd._context = context;
            wnd.UpdateContent();
            wnd.ShowModal();
        }

        public void CreateGUI()
        {
            // Set Window not resizable
            this.minSize = new Vector2(365, 325);
            this.maxSize = this.minSize;

            // Each editor window contains a root VisualElement object
            var root = rootVisualElement;

            // Import UXML
            var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/UI/EditorUI/CreateAnimationDialog.uxml");
            var fromUxml = visualTree.Instantiate();
            root.Add(fromUxml);
        }

        public void UpdateContent()
        {
            var root = rootVisualElement;

            var animationName = root.Q<Label>("Predicate");
            animationName.text = _metadata.ToString();

            // Types list

            _predicateAnimationAttributeList = root.Q<ScrollView>("TypesList");


            var items = _metadata.parameters;


            foreach (var item in items)
            {
                var controller = new PredicateAnimationAttributeController();
                var fromUxml = predicateAnimationAttributeTemplate.Instantiate();
                controller.SetVisualElement(fromUxml);
                controller.SetMetadata(item);
                controller.UpdateContent();
                _predicateAnimationAttributeList.Add(fromUxml);
            }


            // Buttons
            var createButton = root.Q<Button>("CreateButton");
            var cancelButton = root.Q<Button>("CancelButton");

            createButton.clickable.clicked += () =>
            {
                CreateAnimation();
                Close();
            };

            cancelButton.clickable.clicked += () =>
            {
                Close();
            };

        }


        /// <summary>
        /// Creates a new animation object and sets its components.
        /// </summary>
        private void CreateAnimation()
        {
            var predicateName = _metadata.name;
            var attributeTypes = new List<string>();
            var attributes = new List<string>();
            var attributesString = string.Empty;
            var valueType = _metadata.type;

            foreach (var item in _predicateAnimationAttributeList.Children())
            {
                var controller = item.Q<DropdownField>("Attribute");
                attributeTypes.Add(controller.value);
                attributes.Add(controller.label + " " + controller.value);
                attributesString += controller.label + " - " + controller.value + "\n";
            }

            var animationName = AnimationNames.UniqueAnimationName(predicateName, attributeTypes);

            // Load the prefab
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/PDSim/Fluent Animation.prefab");

            // Instantiate the prefab as variant
            var instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            PrefabUtility.UnpackPrefabInstance(instance, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);

            // Set the name of the instance
            instance.name = animationName;

            // Set the position of the instance
            instance.transform.position = Vector3.zero;
            instance.transform.parent = GameObject.Find("Effects Animations").transform;

            // Set the animation script
            var graph = instance.GetComponent<ScriptMachine>().graph;
            graph.title = "Animation Script for predicate: " + predicateName;
            graph.summary = attributesString;

            // Add the units
            var effectEvent = new ActionEffectEvent()
            {
                position = new Vector2(-300, -180),
                ArgumentCount = attributes.Count,
                EffectName = animationName,
                EffectArguments = attributes,
                EffectValueType = valueType
            };

            var contIn = new ControlInputDefinition();
            contIn.key = "Entry";
            contIn.hideLabel = true;

            var contrOut = new ControlOutputDefinition();
            contrOut.key = "Exit";
            contrOut.hideLabel = true;


            // Animation definition subgraph

            var superUnit = new SubgraphUnit();
            superUnit.nest.source = GraphSource.Embed;

            // Create control ports
            superUnit.nest.embed = new FlowGraph()
            {
                controlInputDefinitions = { contIn },
                controlOutputDefinitions = { contrOut }
            };

            // Create value ports

            // For each attribute
            foreach (var a in attributes)
            {
                var el = new ValueInputDefinition();
                el.key = a;
                el.type = typeof(GameObject);
                superUnit.nest.embed.valueInputDefinitions.Add(el);
            }

            // For the value
            var val = new ValueInputDefinition();

            if (valueType == ValueType.Boolean)
            {
                val.key = "Boolean";
                val.type = typeof(bool);
            }
            else if (valueType == ValueType.Real || valueType == ValueType.Int)
            {
                val.key = "Number";
                val.type = typeof(float);
            }
            else
            {
                val.key = "Symbol";
                val.type = typeof(string);
            }
            superUnit.nest.embed.valueInputDefinitions.Add(val);


            // Create the input and output units nodes in the subgraph
            var inputUnit = new GraphInput();
            inputUnit.position = new Vector2(-250, -30);
            var outputUnit = new GraphOutput();
            outputUnit.position = new Vector2(250, -30);

            // Add the value output to the input unit
            superUnit.nest.graph.title = "Animation Definition";
            superUnit.nest.graph.summary = "Click to edit animation definition";
            superUnit.nest.embed.units.Add(inputUnit);
            superUnit.nest.embed.units.Add(outputUnit);


            // Add the EffectEndEvent to the main graph
            var effectEndEvent = new ActionEffectEndEvent()
            {
                position = new Vector2(200, -144),
                EffectName = animationName
            };


            // Add the units to the main graph
            graph.units.Add(effectEvent);
            graph.units.Add(superUnit);
            graph.units.Add(effectEndEvent);

            // Add the connections to the main graph

            // For the controls
            var conn = new ControlConnection(effectEvent.controlOutputs[0], superUnit.controlInputs[0]);
            var conn2 = new ControlConnection(superUnit.controlOutputs[0], effectEndEvent.controlInputs[0]);
            graph.controlConnections.Add(conn);
            graph.controlConnections.Add(conn2);

            // For the values (<= because of the value type)
            for (var i = 0; i <= attributes.Count; i++)
            {
                var valConn = new ValueConnection(effectEvent.valueOutputs[i], superUnit.valueInputs[i]);
                graph.valueConnections.Add(valConn);
            }




            // Add the animation to the context
            if (!_context.AddAnimationData(instance.GetComponent<ScriptMachine>()))
            {
                EditorUtility.DisplayDialog("Error", "Animation already exists", "Ok");
                DestroyImmediate(instance);
            }
        }



    }

}
