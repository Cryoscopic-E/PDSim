using Editor.UI;
using PDSim.Simulation;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Editor.Inspector
{
    /// <summary>
    /// Custom inspector for the FluentAnimation class.
    /// </summary>
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
                // Get the element and its data we want to draw from the list.
                var element = list.serializedProperty.GetArrayElementAtIndex(index);
                var nameProperty = element.FindPropertyRelative("name");
                var machineProperty = element.FindPropertyRelative("machine");

                // Draw the list item as a label field.
                var nameRect = new Rect(rect.x, rect.y, rect.width * 0.45f, rect.height);
                var machineRect = new Rect(rect.x + rect.width * 0.32f, rect.y, rect.width * 0.5f, rect.height);
                var orderRect = new Rect(rect.x + rect.width * 0.95f, rect.y, rect.width * 0.05f, rect.height);
                EditorGUILayout.BeginHorizontal();
                EditorGUI.LabelField(nameRect, nameProperty.stringValue, EditorStyles.boldLabel);
                EditorGUI.PropertyField(machineRect, machineProperty, GUIContent.none);
                EditorGUILayout.EndHorizontal();
            };
            // When user clicks on add button, open the CreateAnimationWindow.
            list.onAddCallback = (ReorderableList List) =>
            {
                EditorApplication.delayCall += CreateAnimation;
            };
            // When user clicks on remove button, remove the animation from the list.
            list.onRemoveCallback = (ReorderableList List) =>
            {
                if (EditorUtility.DisplayDialog("Warning!", "Are you sure you want to delete the animation?", "Yes", "No"))
                {
                    ReorderableList.defaultBehaviours.DoRemoveButton(List);
                    // Remove from scene under Animations
                    var animation = GameObject.Find("Effects Animations");
                    // Find the child with the same name as the animation we want to remove.
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

        private void OnDestroy()
        {
            EditorApplication.delayCall -= CreateAnimation;
        }

        public void CreateAnimation()
        {
            CreateAnimationWindow.ShowAsModal(fluentAnimation.metaData, fluentAnimation);
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