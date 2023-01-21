using PDSim.Components;
using UnityEditor;
using UnityEngine;

namespace Editor.Drawers
{
    [CustomPropertyDrawer(typeof(PdTypedPredicate))]
    public class PdPredicateDrawer : PropertyDrawer
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
                var parameter = parameters.GetArrayElementAtIndex(i);
                var parameterName = parameter.FindPropertyRelative("name").stringValue;
                var parameterType = parameter.FindPropertyRelative("type").stringValue;
                stringParameters += parameterName + " - " + parameterType;
                if (i < parameters.arraySize - 1)
                {
                    stringParameters += ", ";
                }
            }


            // Draw label
            label.text = name + "(" + stringParameters + ")";
            EditorGUI.LabelField(position, label);
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return base.GetPropertyHeight(property, label);
        }
    }
}