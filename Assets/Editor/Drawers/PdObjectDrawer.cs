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
            var name = property.FindPropertyRelative("name");
            var type = property.FindPropertyRelative("type");
            
            // Draw label
            label.text = name.stringValue + " - " + type.stringValue;
            EditorGUI.LabelField(position, label);
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return base.GetPropertyHeight(property, label);
        }
    }
}