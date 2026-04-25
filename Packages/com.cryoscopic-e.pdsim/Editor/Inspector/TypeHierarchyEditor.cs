using PDSim.Components;
using UnityEditor;
using UnityEngine;
using static PDSim.Components.ModelTypes;

namespace PDSim.Editor.Inspector
{
    /// <summary>
    /// Custom inspector for the TypeHierarchy component, visualizing the PDDL type tree.
    /// </summary>
    [CustomEditor(typeof(TypeHierarchy))]
    public class TypeHierarchyEditor : UnityEditor.Editor
    {
        #region Fields
        private TypeHierarchy _typeHierarchy;
        #endregion

        #region Unity Lifecycle
        private void OnEnable()
        {
            _typeHierarchy = (TypeHierarchy)target;
        }

        /// <summary>
        /// Draws the custom inspector GUI for the TypeHierarchy.
        /// </summary>
        public override void OnInspectorGUI()
        {
            EditorGUILayout.BeginVertical();

            EditorGUILayout.LabelField("TYPES DECLARATION", EditorStyles.boldLabel);
            EditorGUILayout.Space();
            EditorGUILayout.BeginVertical();
            DrawNodes(_typeHierarchy.ModelTypes.GetRoot());
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            EditorGUILayout.EndVertical();
        }
        #endregion

        #region Private Methods
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
            if (node.Children.Count > 0)
            {
                GUILayout.Label(label + node.Name);
                var children = node.Children;
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
        #endregion
    }
}