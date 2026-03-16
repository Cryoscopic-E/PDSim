using PDSim.Components;
using UnityEditor;
using UnityEngine;
using static PDSim.Components.ModelTypes;

namespace PDSim.Editor.Inspector
{
    [CustomEditor(typeof(TypeHierarchy))]
    public class TypeHierarchyEditor : UnityEditor.Editor
    {
        private TypeHierarchy typeHierarchy;

        private void OnEnable()
        {
            typeHierarchy = (TypeHierarchy)target;
        }

        public override void OnInspectorGUI()
        {
            EditorGUILayout.BeginVertical();

            EditorGUILayout.LabelField("TYPES DECLARATION", EditorStyles.boldLabel);
            EditorGUILayout.Space();
            EditorGUILayout.BeginVertical();
            DrawNodes(typeHierarchy.modelTypes.GetRoot());
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            EditorGUILayout.EndVertical();
        }


        private void DrawNodes(TypeNode node, int depth = 0)
        {
            if (node == null)
                return;
            var label = string.Empty;
            // Add tabs for each depth
            for (var i = 0; i < depth; i++)
            {
                label += "\t";
            }
            // Add a branching symbol for each depth
            if (depth > 0)
            {
                label += '\u221F'.ToString();
            }
            // Recursively draw the tree
            if (node.children.Count > 0)
            {
                GUILayout.Label(label + node.Name);
                var children = node.children;
                foreach (var c in children)
                {
                    DrawNodes(c, depth + 1);
                }
            }
            else
            {
                GUILayout.Label(label + node.Name, EditorStyles.linkLabel);
            }
        }
    }


}