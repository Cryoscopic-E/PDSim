using PDSim.Components;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using GeTPlan.Core.Models;
using System.Linq;

namespace PDSim.Editor.Inspector
{
    /// <summary>
    /// Custom inspector for the FluentAnimation component.
    /// </summary>
    [CustomEditor(typeof(FluentAnimation))]
    public class FluentAnimationEditor : UnityEditor.Editor
    {
        #region Fields
        private FluentAnimation _fluentAnimation;
        private ReorderableList _list;
        #endregion

        #region Unity Lifecycle
        private void OnEnable()
        {
            _fluentAnimation = (FluentAnimation)target;

            _list = new ReorderableList(serializedObject, serializedObject.FindProperty("animationData"), true, false, true, true);


            _list.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
            {
                // Get the element and its data we want to draw from the list.
                var element = _list.serializedProperty.GetArrayElementAtIndex(index);
                var nameProperty = element.FindPropertyRelative("name");
                var visualizerProperty = element.FindPropertyRelative("visualizer");
                var sceneObjectProperty = element.FindPropertyRelative("sceneObjectReference");
                var classNameProperty = element.FindPropertyRelative("scriptClassName");

                float lineHeight = EditorGUIUtility.singleLineHeight;
                float spacing = 2;

                // Name (First Line)
                var nameRect = new Rect(rect.x, rect.y + spacing, rect.width, lineHeight);
                EditorGUI.LabelField(nameRect, nameProperty.stringValue, EditorStyles.boldLabel);

                // Scene Object (Second Line)
                var sceneObjRect = new Rect(rect.x, rect.y + lineHeight + spacing * 2, rect.width * 0.5f - 5, lineHeight);
                EditorGUI.PropertyField(sceneObjRect, sceneObjectProperty, GUIContent.none);

                // Script/Visualizer (Second Line, Right)
                var scriptRect = new Rect(rect.x + rect.width * 0.5f + 5, rect.y + lineHeight + spacing * 2, rect.width * 0.35f - 10, lineHeight);
                var regenRect = new Rect(rect.x + rect.width * 0.85f, rect.y + lineHeight + spacing * 2, rect.width * 0.15f, lineHeight);
                
                // Read-only property field for visualizer script (auto-attached)
                EditorGUI.PropertyField(scriptRect, visualizerProperty, GUIContent.none);

                if (GUI.Button(regenRect, "Regen"))
                {
                    RegenerateScript(index);
                }
            };

            _list.elementHeightCallback = (int index) =>
            {
                return EditorGUIUtility.singleLineHeight * 2 + 10;
            };

            // When user clicks on add button, open the CreateAnimationWindow.
            _list.onAddCallback = (ReorderableList List) =>
            {
                EditorApplication.delayCall += CreateAnimation;
            };

            // When user clicks on remove button, remove the animation from the list.
            _list.onRemoveCallback = (ReorderableList List) =>
            {
                if (EditorUtility.DisplayDialog("Warning!", "Are you sure you want to delete the animation?", "Yes", "No"))
                {
                    // Destroy the animation object.
                    var data = _fluentAnimation.AnimationDataList[List.index];
                    if (data.SceneObjectReference != null)
                    {
                        DestroyImmediate(data.SceneObjectReference);
                    }
                    // Remove the animation from the list.
                    ReorderableList.defaultBehaviours.DoRemoveButton(List);
                    // Apply the changes to the serialized object.
                    serializedObject.ApplyModifiedProperties();
                }
            };
        }

        private void OnDestroy()
        {
            EditorApplication.delayCall -= CreateAnimation;
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Opens the CreateAnimationWindow to add a new animation.
        /// </summary>
        public void CreateAnimation()
        {
            CreateAnimationWindow.ShowAsModal(_fluentAnimation.MetaData, _fluentAnimation);
        }

        /// <summary>
        /// Draws the custom inspector GUI.
        /// </summary>
        public override void OnInspectorGUI()
        {
            if (_fluentAnimation.MetaData == null)
            {
                EditorGUILayout.HelpBox("Missing MetaData", MessageType.Error);
                return;
            }

            EditorGUILayout.LabelField(_fluentAnimation.MetaData.ToString(), EditorStyles.whiteLargeLabel, GUILayout.Height(20));
            
            EditorGUILayout.Space();
            serializedObject.Update();
            _list.DoLayoutList();
            serializedObject.ApplyModifiedProperties();
        }
        #endregion

        #region Private Methods
        private void RegenerateScript(int index)
        {
            var data = _fluentAnimation.AnimationDataList[index];
            var predicateName = _fluentAnimation.MetaData.Name;
            var attributeTypes = data.Parameters;

            // scriptClassName may be fully-qualified (new) or a bare class name (legacy).
            var fullTypeName = data.ScriptClassName;
            var className = fullTypeName.Contains(".")
                ? fullTypeName.Substring(fullTypeName.LastIndexOf('.') + 1)
                : fullTypeName;

            var sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            var sanitizedSceneName = System.Text.RegularExpressions.Regex.Replace(sceneName, @"[^a-zA-Z0-9_]", "");
            var namespaceName = $"PDSim.Generated.Animations.{sanitizedSceneName}";

            string folderPath = PDSim.Utils.AssetUtils.GetSimulationScriptsPath(sceneName);
            string filePath = System.IO.Path.Combine(folderPath, className + ".cs");

            // Convert Metadata to PredicateDefinition for generator
            var argTypes = attributeTypes.Select(t => new PlanType(t)).ToArray();
            var predicate = new PredicateDefinition(predicateName, argTypes);

            string code = PDSimAPI.Generators.FluentScriptGenerator.GenerateUnityScript(predicate, className, namespaceName);
            System.IO.File.WriteAllText(filePath, code);
            AssetDatabase.Refresh();
            Debug.Log($"[PDSim] Regenerated C# Script: {filePath}");
        }
        #endregion
    }
}
