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

        private Toggle _isNegated;

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

            // Negated

            if (_metadata.type != ValueType.Boolean)
                root.Q<VisualElement>("NegatedContainer").style.display = DisplayStyle.None;
            else
                root.Q<VisualElement>("NegatedContainer").style.display = DisplayStyle.Flex;

            _isNegated = root.Q<Toggle>("Negated");


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
            var negated = _isNegated.value;
            var attributeTypes = new List<string>();
            var attributes = new List<string>();
            var attributesString = string.Empty;

            foreach (var item in _predicateAnimationAttributeList.Children())
            {
                var controller = item.Q<DropdownField>("Attribute");
                attributeTypes.Add(controller.value);
                attributes.Add(controller.label + " " + controller.value);
                attributesString += controller.label + " - " + controller.value + "\n";
            }
            // Create a unique name for the animation, in format: "Negated_PredicateName_AttributeType1_AttributeType2_..."
            var animationName = "";
            if (_metadata.type == ValueType.Boolean)
                animationName = AnimationNames.UniqueBooleanAnimationName(PdSimAtom.Boolean(!negated), predicateName, attributeTypes);
            else
                animationName = AnimationNames.UniqueNumericAnimationName(predicateName, attributeTypes);

            // Load the prefab
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/PDSim/Fluent Animation.prefab");

            // Instantiate the prefab as variant
            var instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            PrefabUtility.UnpackPrefabInstance(instance, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);

            // Set the name of the instance
            instance.name = animationName;

            // Set the position of the instance
            instance.transform.position = Vector3.zero;
            instance.transform.parent = GameObject.Find("Animations").transform;

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
                EffectArguments = attributes
            };

            var contIn = new ControlInputDefinition();
            contIn.key = "Entry";
            contIn.hideLabel = true;

            var contrOut = new ControlOutputDefinition();
            contrOut.key = "Exit";
            contrOut.hideLabel = true;

            var superUnit = new SubgraphUnit();
            superUnit.nest.source = GraphSource.Embed;
            superUnit.nest.embed = new FlowGraph()
            {
                controlInputDefinitions = { contIn },
                controlOutputDefinitions = { contrOut }
            };


            foreach (var a in attributes)
            {
                var el = new ValueInputDefinition();
                el.key = a;
                el.type = typeof(GameObject);
                superUnit.nest.embed.valueInputDefinitions.Add(el);
            }


            var inputUnit = new GraphInput();
            inputUnit.position = new Vector2(-250, -30);
            var outputUnit = new GraphOutput();
            outputUnit.position = new Vector2(250, -30);


            superUnit.nest.graph.title = "Animation Definition";
            superUnit.nest.graph.summary = "Click to edit animation definition";
            superUnit.nest.embed.units.Add(inputUnit);
            superUnit.nest.embed.units.Add(outputUnit);


            var effectEndEvent = new ActionEffectEndEvent()
            {
                position = new Vector2(200, -144),
                EffectName = animationName
            };


            graph.units.Add(effectEvent);
            graph.units.Add(superUnit);
            graph.units.Add(effectEndEvent);

            var conn = new ControlConnection(effectEvent.controlOutputs[0], superUnit.controlInputs[0]);
            var conn2 = new ControlConnection(superUnit.controlOutputs[0], effectEndEvent.controlInputs[0]);
            graph.controlConnections.Add(conn);
            graph.controlConnections.Add(conn2);


            for (var i = 0; i < attributes.Count; i++)
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
