using UnityEditor;
using PDSim.Protobuf;
using UnityEngine;

namespace Editor.Inspector
{
    /// <summary>
    /// Custom inspector for the Problem class.
    /// </summary>
    [CustomEditor(typeof(PdSimProblem))]
    public class PdSimProblemEditor : UnityEditor.Editor
    {
        private PdSimProblem _problem;

        private void OnEnable()
        {
            _problem = (PdSimProblem)target;
        }

        public override void OnInspectorGUI()
        {
            EditorGUILayout.BeginVertical();

            // DOMAIN NAME
            EditorGUILayout.LabelField("DOMAIN NAME", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            EditorGUILayout.LabelField(_problem.DomainName, EditorStyles.largeLabel, GUILayout.ExpandWidth(false));
            EditorGUI.indentLevel--;
            EditorGUILayout.Space();


            // PROBLEM NAME
            EditorGUILayout.LabelField("PROBLEM NAME", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            EditorGUILayout.LabelField(_problem.ProblemName, EditorStyles.largeLabel, GUILayout.ExpandWidth(false));
            EditorGUI.indentLevel--;
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

            // TYPES DECLARATION
            var typesDeclaration = _problem.TypesDeclaration;

            EditorGUILayout.LabelField("TYPES DECLARATION", EditorStyles.boldLabel);
            EditorGUILayout.Space();
            EditorGUILayout.BeginVertical();
            DrawNodes(typesDeclaration.TypeTree.GetRoot());
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

            // FLUENTS
            var fluents = _problem.Fluents;

            EditorGUILayout.LabelField("FLUENTS", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            EditorGUILayout.BeginVertical();
            foreach (var fluent in fluents)
            {
                EditorGUILayout.LabelField(fluent.ToString(), EditorStyles.largeLabel, GUILayout.ExpandWidth(false));
            }
            EditorGUILayout.EndVertical();
            EditorGUI.indentLevel--;

            EditorGUILayout.Space();


            // OBJECTS
            var objects = _problem.Objects;

            EditorGUILayout.LabelField("OBJECTS", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            EditorGUILayout.BeginVertical();
            foreach (var obj in objects)
            {
                EditorGUILayout.LabelField(obj.ToString(), EditorStyles.largeLabel, GUILayout.ExpandWidth(false));
            }
            EditorGUILayout.EndVertical();
            EditorGUI.indentLevel--;

            EditorGUILayout.EndVertical();
        }

        private void DrawNodes(TypeTree.TypeNode node, int depth = 0)
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