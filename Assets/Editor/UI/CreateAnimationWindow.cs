using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using PDSim.Components;
using Unity.VisualScripting;
using PDSim.Animation;
using PDSim.Simulation;

namespace Editor.UI
{
    public class CreateAnimationWindow : EditorWindow
    {
        public VisualTreeAsset predicateAnimationAttributeTemplate;

        private PdTypedPredicate _metadata;

        private Toggle _isNegated;

        private ScrollView _predicateAnimationAttributeList;

        private FluentAnimation _context;

        public static void ShowAsModal(PdTypedPredicate metadata, FluentAnimation context)
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
            var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/UI Toolkit/EditorUI/CreateAnimationDialog.uxml");
            var fromUxml = visualTree.Instantiate();
            root.Add(fromUxml);
        }

        public void UpdateContent()
        {
            var root = rootVisualElement;

            var animationName = root.Q<Label>("Predicate");
            animationName.text = _metadata.ToString();

            // Negated
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

        private string UniqueAnimationName(bool negated, string predicateName, List<string> attributeTypes)
        {
            var animationName = predicateName;
            if (negated)
                animationName = "NOT_" + animationName;

            foreach (var item in attributeTypes)
            {
                animationName += "_" + item;
            }

            return animationName;
        }

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

            var animationName = UniqueAnimationName(negated, predicateName, attributeTypes);



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

            var graph = instance.GetComponent<ScriptMachine>().graph;
            graph.title = "Animation Script for predicate: " + predicateName;
            graph.summary = attributesString;


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
