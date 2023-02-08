using PDSim.Components;
using UnityEditor;
using UnityEngine;

namespace Editor.Drawers
{
    [CustomPropertyDrawer(typeof(PdObject))]
    public class PdObjectDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // Get properties
            var name = property.FindPropertyRelative("name").stringValue;
            var type = property.FindPropertyRelative("type").stringValue;
            
            // Draw label
            label.text = name + " - " + type;
            EditorGUI.LabelField(position, label);
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return base.GetPropertyHeight(property, label);
        }
    }
}