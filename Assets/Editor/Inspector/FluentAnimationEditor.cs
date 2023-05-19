using PDSim.Simulation;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Editor.Inspector
{
    [CustomEditor(typeof(FluentAnimation))]
    public class FluentAnimationEditor : UnityEditor.Editor
    {
        private ReorderableList list;

        private void OnEnable()
        {
            list = new ReorderableList(serializedObject, serializedObject.FindProperty("data"), true, false, true, true);

            list.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
            {
                var element = list.serializedProperty.GetArrayElementAtIndex(index);
                rect.y += 2;
                var nameProperty = element.FindPropertyRelative("name");
                var machineProperty = element.FindPropertyRelative("machine");
                var orderProperty = element.FindPropertyRelative("order");

                var nameRect = new Rect(rect.x, rect.y, rect.width * 0.45f, rect.height);
                var machineRect = new Rect(rect.x + rect.width * 0.32f, rect.y, rect.width * 0.5f, rect.height);
                var orderRect = new Rect(rect.x + rect.width * 0.82f, rect.y, rect.width * 0.05f, rect.height);

                EditorGUILayout.BeginHorizontal();
                EditorGUI.LabelField(nameRect, nameProperty.stringValue, EditorStyles.boldLabel);
                EditorGUI.PropertyField(machineRect, machineProperty, GUIContent.none);
                EditorGUI.PropertyField(orderRect, orderProperty, GUIContent.none);
                EditorGUILayout.EndHorizontal();
            };

            list.onAddCallback = (ReorderableList List) =>
            {
                var index = List.serializedProperty.arraySize;
                List.serializedProperty.arraySize++;
                List.index = index;
                var element = List.serializedProperty.GetArrayElementAtIndex(index);
                element.FindPropertyRelative("name").stringValue = "New Animation";
                element.FindPropertyRelative("machine").objectReferenceValue = null;
                element.FindPropertyRelative("order").intValue = 0;
            };

            list.onRemoveCallback = (ReorderableList List) =>
            {
                if (EditorUtility.DisplayDialog("Warning!", "Are you sure you want to delete the animation?", "Yes", "No"))
                {
                    ReorderableList.defaultBehaviours.DoRemoveButton(List);
                }
            };
        }

        public override void OnInspectorGUI()
        {
            var flentName = serializedObject.FindProperty("fluentName");
            EditorGUILayout.LabelField(flentName.stringValue, EditorStyles.whiteLargeLabel);

            serializedObject.Update();
            list.DoLayoutList();
            serializedObject.ApplyModifiedProperties();
        }
    }
}