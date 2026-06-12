using PDSim.Components;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using GeTPlan.Core.Models;
using System.Linq;

namespace PDSim.Editor.Inspector
{
    /// <summary>
    /// Custom inspector for the ActionAnimation component.
    /// </summary>
    [CustomEditor(typeof(ActionAnimation))]
    public class ActionAnimationEditor : UnityEditor.Editor
    {
        #region Fields
        private ActionAnimation _actionAnimation;
        private ReorderableList _list;
        #endregion

        #region Unity Lifecycle
        private void OnEnable()
        {
            _actionAnimation = (ActionAnimation)target;

            _list = new ReorderableList(serializedObject, serializedObject.FindProperty("AnimationDataList"), true, false, true, true);

            _list.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
            {
                var element = _list.serializedProperty.GetArrayElementAtIndex(index);
                var nameProperty = element.FindPropertyRelative("Name");
                var visualizerProperty = element.FindPropertyRelative("Visualizer");
                var sceneObjectProperty = element.FindPropertyRelative("SceneObjectReference");

                float lineHeight = EditorGUIUtility.singleLineHeight;
                float spacing = 2;

                var nameRect = new Rect(rect.x, rect.y + spacing, rect.width, lineHeight);
                EditorGUI.LabelField(nameRect, nameProperty.stringValue, EditorStyles.boldLabel);

                var sceneObjRect = new Rect(rect.x, rect.y + lineHeight + spacing * 2, rect.width * 0.5f - 5, lineHeight);
                EditorGUI.PropertyField(sceneObjRect, sceneObjectProperty, GUIContent.none);

                var scriptRect = new Rect(rect.x + rect.width * 0.5f + 5, rect.y + lineHeight + spacing * 2, rect.width * 0.35f - 10, lineHeight);
                var regenRect = new Rect(rect.x + rect.width * 0.85f, rect.y + lineHeight + spacing * 2, rect.width * 0.15f, lineHeight);

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

            _list.onAddCallback = (ReorderableList List) =>
            {
                EditorApplication.delayCall += CreateAnimation;
            };

            _list.onRemoveCallback = (ReorderableList List) =>
            {
                if (EditorUtility.DisplayDialog("Warning!", "Are you sure you want to delete the animation?", "Yes", "No"))
                {
                    var data = _actionAnimation.AnimationDataList[List.index];

                    // Delete the generated script file.
                    if (!string.IsNullOrEmpty(data.ScriptClassName))
                    {
                        var fullTypeName = data.ScriptClassName;
                        var className = fullTypeName.Contains(".")
                            ? fullTypeName.Substring(fullTypeName.LastIndexOf('.') + 1)
                            : fullTypeName;
                        var sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
                        string filePath = PDSim.Utils.AssetUtils.GetSimulationAnimationsPath(sceneName) + "/Actions/" + className + ".cs";
                        if (System.IO.File.Exists(filePath))
                        {
                            AssetDatabase.DeleteAsset(filePath);
                            Debug.Log($"[PDSim] Deleted script: {filePath}");
                        }
                    }

                    // Destroy the animation scene object.
                    if (data.SceneObjectReference != null)
                        DestroyImmediate(data.SceneObjectReference);

                    ReorderableList.defaultBehaviours.DoRemoveButton(List);
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
        /// Opens the CreateActionAnimationWindow to add a new animation.
        /// </summary>
        public void CreateAnimation()
        {
            CreateActionAnimationWindow.ShowAsModal(_actionAnimation.MetaData, _actionAnimation);
        }

        /// <summary>
        /// Draws the custom inspector GUI.
        /// </summary>
        public override void OnInspectorGUI()
        {
            if (_actionAnimation.MetaData == null)
            {
                EditorGUILayout.HelpBox("Missing MetaData", MessageType.Error);
                return;
            }

            EditorGUILayout.LabelField(_actionAnimation.MetaData.ToString(), EditorStyles.whiteLargeLabel, GUILayout.Height(20));

            EditorGUILayout.Space();
            serializedObject.Update();
            _list.DoLayoutList();
            serializedObject.ApplyModifiedProperties();
        }
        #endregion

        #region Private Methods
        private void RegenerateScript(int index)
        {
            var data = _actionAnimation.AnimationDataList[index];
            var actionName = _actionAnimation.MetaData.Name;
            var attributeTypes = data.Parameters;

            var fullTypeName = data.ScriptClassName;
            var className = fullTypeName.Contains(".")
                ? fullTypeName.Substring(fullTypeName.LastIndexOf('.') + 1)
                : fullTypeName;

            var sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            var sanitizedSceneName = System.Text.RegularExpressions.Regex.Replace(sceneName, @"[^a-zA-Z0-9_]", "");
            var namespaceName = $"PDSim.Generated.Animations.{sanitizedSceneName}";

            string folderPath = PDSim.Utils.AssetUtils.GetSimulationAnimationsPath(sceneName) + "/Actions";
            string filePath = System.IO.Path.Combine(folderPath, className + ".cs");

            var paramDefs = _actionAnimation.MetaData.ParametersNames
                .Zip(_actionAnimation.MetaData.ParametersTypes, (name, type) => (name, type))
                .ToList();

            string code = PDSimAPI.Generators.ActionScriptGenerator.GenerateActionAnimationUnityScript(
                actionName, paramDefs, attributeTypes, className, namespaceName);
            System.IO.File.WriteAllText(filePath, code);
            AssetDatabase.Refresh();
            Debug.Log($"[PDSim] Regenerated action animation script: {filePath}");
        }
        #endregion
    }
}
