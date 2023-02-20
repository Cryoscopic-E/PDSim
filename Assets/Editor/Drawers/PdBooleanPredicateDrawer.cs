using PDSim.Components;
using UnityEditor;
using UnityEngine;

namespace Editor.Drawers
{
    [CustomPropertyDrawer(typeof(PdBooleanPredicate))]
    public class PdBooleanPredicateDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // Get properties
            var name = property.FindPropertyRelative("name").stringValue;
            var attributes = property.FindPropertyRelative("attributes");
            var value = property.FindPropertyRelative("value").boolValue;
            // get parameter names
            var stringAttributes = "";
            
            for (var i = 0; i < attributes.arraySize; ++i)
            {
                var att = attributes.GetArrayElementAtIndex(i);
                stringAttributes += att.stringValue;
                if (i < attributes.arraySize - 1)
                {
                    stringAttributes += ", ";
                }
            }


            // Draw label
            label.text = name + " ( " + stringAttributes + " )" + " :: " + value.ToString().ToUpper(); 
            EditorGUI.LabelField(position, label);
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return base.GetPropertyHeight(property, label);
        }
    }
}