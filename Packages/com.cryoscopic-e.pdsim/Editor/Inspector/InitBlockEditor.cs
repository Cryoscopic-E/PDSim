using PDSim.Components;
using UnityEditor;
using UnityEngine;
using UnityEditorInternal;

namespace PDSim.Editor.Inspector
{
    /// <summary>
    /// Custom inspector for the InitBlock component, showing the initial world state.
    /// </summary>
    [CustomEditor(typeof(InitBlock))]
    public class InitBlockEditor : UnityEditor.Editor
    {
        #region Fields
        private InitBlock _initBlock;
        private ReorderableList _initList;
        #endregion

        #region Unity Lifecycle
        private void OnEnable()
        {
            _initBlock = (InitBlock)target;

            _initList = new ReorderableList(serializedObject, serializedObject.FindProperty("Components"), true, false, false, false);

            _initList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
            {
                var initString = _initBlock.Components[index].ToString();

                EditorGUI.LabelField(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight), initString);
            };
        }

        /// <summary>
        /// Draws the custom inspector GUI for the InitBlock.
        /// </summary>
        public override void OnInspectorGUI()
        {
            EditorGUILayout.LabelField("INITIAL STATE", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            EditorGUILayout.BeginVertical();
            _initList.DoLayoutList();
            EditorGUILayout.EndVertical();
            EditorGUI.indentLevel--;

            serializedObject.ApplyModifiedProperties();
        }
        #endregion
    }
}

