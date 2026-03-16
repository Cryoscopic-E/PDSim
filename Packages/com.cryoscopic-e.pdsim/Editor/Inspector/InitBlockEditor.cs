using PDSim.Components;
using UnityEditor;
using UnityEngine;
using UnityEditorInternal;

namespace PDSim.Editor.Inspector
{
    [CustomEditor(typeof(InitBlock))]
    public class InitBlockEditor : UnityEditor.Editor
    {
        private InitBlock initBlock;
        private ReorderableList _initList;

        private void OnEnable()
        {
            initBlock = (InitBlock)target;

            _initList = new ReorderableList(serializedObject, serializedObject.FindProperty("Components"), true, false, false, false);

            _initList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
            {
                var initString = initBlock.Components[index].ToString();

                EditorGUI.LabelField(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight), initString);
            };
        }
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
    }

}

