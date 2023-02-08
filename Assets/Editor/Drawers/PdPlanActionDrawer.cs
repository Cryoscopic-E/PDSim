using PDSim.Components;
using UnityEditor;
using UnityEngine;

namespace Editor.Drawers
{
    [CustomPropertyDrawer(typeof(PdPlanAction))]
    public class PdPlanActionDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // Get properties
            var name = property.FindPropertyRelative("name").stringValue;
            var parameters = property.FindPropertyRelative("parameters");
            
            // get parameter names
            var stringParameters = "";
            
            for (var i = 0; i < parameters.arraySize; i++)
            {
                var at = parameters.GetArrayElementAtIndex(i);
                stringParameters += at.stringValue;
                if (i < parameters.arraySize - 1)
                {
                    stringParameters += ", ";
                }
            }


            // Draw label
            label.text = name + " (" + stringParameters + ")";
            EditorGUI.LabelField(position, label);
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return base.GetPropertyHeight(property, label);
        }
    }
}