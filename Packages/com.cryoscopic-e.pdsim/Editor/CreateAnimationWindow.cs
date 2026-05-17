using System.Collections.Generic;
using System.Linq;
using PDSim.Components;
using GeTPlan.Core.Models;
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
        #region Fields
        /// <summary>
        /// Template for the predicate animation attribute UI.
        /// </summary>
        public VisualTreeAsset PredicateAnimationAttributeTemplate;

        private FluentAnimation.FluentMetadata _metadata;
        private ScrollView _predicateAnimationAttributeList;
        private FluentAnimation _context;
        #endregion

        #region Public Methods
        /// <summary>
        /// Shows the CreateAnimationWindow as a modal window.
        /// </summary>
        /// <param name="metadata">The fluent metadata.</param>
        /// <param name="context">The fluent animation context.</param>
        public static void ShowAsModal(FluentAnimation.FluentMetadata metadata, FluentAnimation context)
        {
            var wnd = GetWindow<CreateAnimationWindow>();
            wnd.titleContent = new GUIContent("Create New Animation");
            wnd._metadata = metadata;
            wnd._context = context;
            wnd.UpdateContent();
            wnd.ShowModal();
        }

        /// <summary>
        /// Called when the window is created to initialize the UI.
        /// </summary>
        public void CreateGUI()
        {
            // Set Window not resizable
            this.minSize = new Vector2(365, 325);
            this.maxSize = this.minSize;

            // Each editor window contains a root VisualElement object
            var root = rootVisualElement;

            // Import UXML
            var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(CommonPaths.AnimationDialogUI);
            var fromUxml = visualTree.Instantiate();
            root.Add(fromUxml);

            // Load the attribute template
            PredicateAnimationAttributeTemplate = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(CommonPaths.PredicateAnimationAttributeUI);
        }

        /// <summary>
        /// Updates the window content based on the provided metadata.
        /// </summary>
        public void UpdateContent()
        {
            var root = rootVisualElement;

            if (PredicateAnimationAttributeTemplate == null)
            {
                PredicateAnimationAttributeTemplate = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(CommonPaths.PredicateAnimationAttributeUI);
            }

            var animationName = root.Q<Label>("Predicate");
            animationName.text = _metadata.ToString();

            // Types list

            _predicateAnimationAttributeList = root.Q<ScrollView>("TypesList");


            var items = _metadata.ParametersTypes;

            for (var i = 0; i < items.Count; i++)
            {
                var item = items[i];
                var controller = new PredicateAnimationAttributeController();
                var fromUxml = PredicateAnimationAttributeTemplate.Instantiate();
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
        #endregion

        #region Private Methods
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

            // Create a new scene GameObject to represent this animation variant.
            var instance = new GameObject(animationName);
            Undo.RegisterCreatedObjectUndo(instance, "Create Animation Variant");

            // Set the position of the instance
            instance.transform.position = Vector3.zero;
            instance.transform.parent = Animations.Instance.transform;

            // Generate the C# visualizer script if it doesn't already exist.
            // Everything should be handled in the DLL library
            var sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            var sanitizedSceneName = System.Text.RegularExpressions.Regex.Replace(sceneName, @"[^a-zA-Z0-9_]", "");
            var namespaceName = $"PDSim.Generated.Animations.{sanitizedSceneName}";

            string folderPath = AssetUtils.GetSimulationAnimationsPath(sceneName);
            if (!System.IO.Directory.Exists(folderPath))
            {
                System.IO.Directory.CreateDirectory(folderPath);
            }

            // Build PredicateDefinition with real parameter names from metadata
            var parameters = attributeTypes.Zip(_metadata.ParametersNames,
                (type, name) => new PredicateParameter(name, new PlanType(type)));
            var predicate = new PredicateDefinition(predicateName, _metadata.FluentValueType ?? "bool", parameters);

            string className = PDSimAPI.Generators.FluentScriptGenerator.GetVisualizerClassName(predicate);
            string fullTypeName = $"{namespaceName}.{className}";
            string filePath = System.IO.Path.Combine(folderPath, className + ".cs");

            // Only generate if it doesn't exist yet
            if (!System.IO.File.Exists(filePath))
            {
                string code = PDSimAPI.Generators.FluentScriptGenerator.GenerateUnityScript(predicate, className, namespaceName);
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
        #endregion
    }
}
