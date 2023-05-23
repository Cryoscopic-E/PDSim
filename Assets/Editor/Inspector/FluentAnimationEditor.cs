using PDSim.Simulation;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using Editor.UI;

namespace Editor.Inspector
{
    [CustomEditor(typeof(FluentAnimation))]
    public class FluentAnimationEditor : UnityEditor.Editor
    {
        private FluentAnimation fluentAnimation;
        private ReorderableList list;

        private void OnEnable()
        {
            fluentAnimation = (FluentAnimation)target;

            list = new ReorderableList(serializedObject, serializedObject.FindProperty("animationData"), true, false, true, true);

            list.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
            {
                var element = list.serializedProperty.GetArrayElementAtIndex(index);
                rect.y += 2;
                var nameProperty = element.FindPropertyRelative("name");
                var machineProperty = element.FindPropertyRelative("machine");
                var orderProperty = element.FindPropertyRelative("order");

                var nameRect = new Rect(rect.x, rect.y, rect.width * 0.45f, rect.height);
                var machineRect = new Rect(rect.x + rect.width * 0.32f, rect.y, rect.width * 0.5f, rect.height);
                var orderRect = new Rect(rect.x + rect.width * 0.95f, rect.y, rect.width * 0.05f, rect.height);

                EditorGUILayout.BeginHorizontal();
                EditorGUI.LabelField(nameRect, nameProperty.stringValue, EditorStyles.boldLabel);
                EditorGUI.PropertyField(machineRect, machineProperty, GUIContent.none);
                EditorGUI.PropertyField(orderRect, orderProperty, GUIContent.none);
                EditorGUILayout.EndHorizontal();
            };

            list.onAddCallback = (ReorderableList List) =>
            {
                CreateAnimationWindow.ShowAsModal(fluentAnimation.metaData, fluentAnimation);
            };

            list.onRemoveCallback = (ReorderableList List) =>
            {
                if (EditorUtility.DisplayDialog("Warning!", "Are you sure you want to delete the animation?", "Yes", "No"))
                {
                    ReorderableList.defaultBehaviours.DoRemoveButton(List);
                    // Remove from scene under Animations
                    var animation = GameObject.Find("Animations");

                    for (var i = 0; i < animation.transform.childCount; i++)
                    {
                        var child = animation.transform.GetChild(i);
                        var selectedIndex = List.selectedIndices[0];
                        if (child.name == fluentAnimation.animationData[selectedIndex].name)
                        {
                            Undo.DestroyObjectImmediate(child.gameObject);
                        }
                    }
                }
            };
        }

        public override void OnInspectorGUI()
        {
            EditorGUILayout.LabelField(fluentAnimation.metaData.ToString(), EditorStyles.whiteLargeLabel, GUILayout.Height(20));
            EditorGUILayout.Space();
            serializedObject.Update();
            list.DoLayoutList();
            serializedObject.ApplyModifiedProperties();
        }
    }
}