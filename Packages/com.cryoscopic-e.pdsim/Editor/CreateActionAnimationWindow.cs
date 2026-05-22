using System.Collections.Generic;
using System.Linq;
using PDSim.Components;
using PDSim.Utils;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace PDSim.Editor
{
    /// <summary>
    /// Custom editor window for creating new action animations.
    /// </summary>
    public class CreateActionAnimationWindow : EditorWindow
    {
        #region Fields
        /// <summary>
        /// Template for the predicate animation attribute UI (reused for action parameters).
        /// </summary>
        public VisualTreeAsset PredicateAnimationAttributeTemplate;

        private ActionAnimation.ActionMetadata _metadata;
        private ScrollView _predicateAnimationAttributeList;
        private ActionAnimation _context;
        #endregion

        #region Public Methods
        /// <summary>
        /// Shows the CreateActionAnimationWindow as a modal window.
        /// </summary>
        /// <param name="metadata">The action metadata.</param>
        /// <param name="context">The action animation context.</param>
        public static void ShowAsModal(ActionAnimation.ActionMetadata metadata, ActionAnimation context)
        {
            var wnd = GetWindow<CreateActionAnimationWindow>();
            wnd.titleContent = new GUIContent("Create New Action Animation");
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
            this.minSize = new Vector2(365, 325);
            this.maxSize = this.minSize;

            var root = rootVisualElement;

            // Reuse the same UXML as the fluent animation window — same structure
            var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(CommonPaths.AnimationDialogUI);
            var fromUxml = visualTree.Instantiate();
            root.Add(fromUxml);

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
        /// Creates a new action animation variant scene object and generates the visualizer script.
        /// </summary>
        private void CreateAnimation()
        {
            var actionName = _metadata.Name;
            var attributeTypes = new List<string>();

            foreach (var item in _predicateAnimationAttributeList.Children())
            {
                var controller = item.Q<DropdownField>("Attribute");
                attributeTypes.Add(controller.value);
            }

            var animationName = AnimationNames.UniqueAnimationName(actionName, attributeTypes);

            var instance = new GameObject(animationName);
            Undo.RegisterCreatedObjectUndo(instance, "Create Action Animation Variant");
            instance.transform.position = Vector3.zero;
            instance.transform.parent = ActionAnimations.Instance.transform;

            var sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            var sanitizedSceneName = System.Text.RegularExpressions.Regex.Replace(sceneName, @"[^a-zA-Z0-9_]", "");
            var namespaceName = $"PDSim.Generated.Animations.{sanitizedSceneName}";

            string folderPath = AssetUtils.GetSimulationAnimationsPath(sceneName) + "/Actions";
            if (!System.IO.Directory.Exists(folderPath))
            {
                System.IO.Directory.CreateDirectory(folderPath);
            }

            // Build parameter definitions from metadata
            var paramDefs = _metadata.ParametersNames
                .Zip(attributeTypes, (name, type) => (name, type))
                .ToList();

            string className = PDSimAPI.Generators.ActionScriptGenerator.GetActionAnimationVisualizerClassName(actionName, attributeTypes);
            string fullTypeName = $"{namespaceName}.{className}";
            string filePath = System.IO.Path.Combine(folderPath, className + ".cs");

            if (!System.IO.File.Exists(filePath))
            {
                string code = PDSimAPI.Generators.ActionScriptGenerator.GenerateActionAnimationUnityScript(
                    actionName, paramDefs, attributeTypes, className, namespaceName);
                System.IO.File.WriteAllText(filePath, code);
                AssetDatabase.Refresh();
                Debug.Log($"[PDSim] Generated action animation script: {filePath}");
            }

            if (!_context.AddAnimationData(animationName, attributeTypes, instance, fullTypeName))
            {
                EditorUtility.DisplayDialog("Error", $"Animation '{animationName}' already exists for action '{actionName}'.", "Ok");
                DestroyImmediate(instance);
                return;
            }

            Debug.Log($"[PDSim] Created action animation variant '{animationName}' for action '{actionName}'.");
            Debug.LogWarning($"[PDSim] Please attach the script '{className}' to the GameObject '{animationName}' in the scene once compilation finishes.");

            EditorUtility.SetDirty(_context);
        }
        #endregion
    }
}
