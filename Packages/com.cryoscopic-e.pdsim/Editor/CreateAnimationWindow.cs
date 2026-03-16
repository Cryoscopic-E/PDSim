using System.Collections.Generic;
using PDSim.Components;
using GeTModel;
using PDSim.Utils;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace PDSim.Editor
{
    /// <summary>
    /// Custom editor window for creating new animations.
    /// </summary>
    public class CreateAnimationWindow : EditorWindow
    {
        public VisualTreeAsset predicateAnimationAttributeTemplate;

        private FluentAnimation.FluentMetadata _metadata;

        private ScrollView _predicateAnimationAttributeList;

        private FluentAnimation _context;

        public static void ShowAsModal(FluentAnimation.FluentMetadata metadata, FluentAnimation context)
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
            var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(CommonPaths.ANIMATION_DIALOG_UI);
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


            var items = _metadata.ParametersTypes;

            for (var i = 0; i < items.Count; i++)
            {
                var item = items[i];
                var controller = new PredicateAnimationAttributeController();
                var fromUxml = predicateAnimationAttributeTemplate.Instantiate();
                controller.SetVisualElement(fromUxml);
                controller.SetMetadata(_metadata.ParametersNames[i], item);
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
            var predicateName = _metadata.Name;
            var attributeTypes = new List<string>();
            var attributes = new List<string>();

            foreach (var item in _predicateAnimationAttributeList.Children())
            {
                var controller = item.Q<DropdownField>("Attribute");
                attributeTypes.Add(controller.value);
                attributes.Add(controller.label + " " + controller.value);
            }

            var animationName = AnimationNames.UniqueAnimationName(predicateName, attributeTypes);

            // --- CREATE NEW GAMEOBJECT (No longer using Prefab) ---
            var instance = new GameObject(animationName);
            Undo.RegisterCreatedObjectUndo(instance, "Create Animation Variant");

            // Set the position of the instance
            instance.transform.position = Vector3.zero;
            instance.transform.parent = Animations.Instance.transform;

            // --- C# SCRIPT GENERATION ---
            // Everything should be handled in the DLL library
            var sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            var sanitizedSceneName = System.Text.RegularExpressions.Regex.Replace(sceneName, @"[^a-zA-Z0-9_]", "");
            var namespaceName = $"PDSim.Generated.Animations.{sanitizedSceneName}";

            string folderPath = AssetUtils.GetSimulationAnimationsPath(sceneName);
            if (!System.IO.Directory.Exists(folderPath))
            {
                System.IO.Directory.CreateDirectory(folderPath);
            }

            string className = PDSimAPI.Generators.FluentScriptGenerator.GetVisualizerClassName(predicateName, attributeTypes);
            string fullTypeName = $"{namespaceName}.{className}";
            string filePath = System.IO.Path.Combine(folderPath, className + ".cs");

            // Convert Metadata to GeTFluent
            var parameters = new List<GeTParameter>();
            if (_metadata.ParametersNames != null)
            {
                for (int i = 0; i < _metadata.ParametersNames.Count; i++)
                {
                    string pName = _metadata.ParametersNames[i];
                    string pType = i < attributeTypes.Count ? attributeTypes[i] : "object";
                    parameters.Add(new GeTParameter(pName, pType));
                }
            }
            var fluent = new GeTFluent(predicateName, _metadata.FluentValueType, parameters);

            // Only generate if it doesn't exist yet
            if (!System.IO.File.Exists(filePath))
            {
                string code = PDSimAPI.Generators.FluentScriptGenerator.GenerateUnityScript(fluent, className, namespaceName);
                System.IO.File.WriteAllText(filePath, code);
                AssetDatabase.Refresh();
                Debug.Log($"[PDSim] Generated C# Script: {filePath}");
            }

            // Add the animation to the context (FluentAnimation component)
            // scriptClassName stores the fully-qualified type name for direct assembly lookup.
            if (!_context.AddAnimationData(animationName, attributeTypes, instance, fullTypeName))
            {
                EditorUtility.DisplayDialog("Error", $"Animation '{animationName}' already exists for fluent '{predicateName}'.", "Ok");
                DestroyImmediate(instance);
                return;
            }

            // Note: IFluentVisualizer won't be found until Unity finishes compiling the new script.
            Debug.Log($"[PDSim] Created animation variant '{animationName}' for fluent '{predicateName}'.");
            Debug.LogWarning($"[PDSim] Please attach the script '{className}' to the GameObject '{animationName}' in the scene once compilation finishes.");

            EditorUtility.SetDirty(_context);
        }

    }

}
