using PDSim.Protobuf;
using UnityEditor;
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
            EditorGUILayout.LabelField(_problem.domainName, EditorStyles.largeLabel);
            EditorGUI.indentLevel--;
            EditorGUILayout.Space();


            // PROBLEM NAME
            EditorGUILayout.LabelField("PROBLEM NAME", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            EditorGUILayout.LabelField(_problem.problemName, EditorStyles.largeLabel);
            EditorGUI.indentLevel--;
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

            // TYPES DECLARATION
            var typesDeclaration = _problem.typesDeclaration;

            EditorGUILayout.LabelField("TYPES DECLARATION", EditorStyles.boldLabel);
            EditorGUILayout.Space();
            EditorGUILayout.BeginVertical();
            DrawNodes(typesDeclaration.GetRoot());
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

            // FLUENTS
            var fluents = _problem.fluents;

            EditorGUILayout.LabelField("FLUENTS", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            EditorGUILayout.BeginVertical();
            foreach (var fluent in fluents)
            {
                EditorGUILayout.LabelField(fluent.ToString(), EditorStyles.largeLabel);
            }
            EditorGUILayout.EndVertical();
            EditorGUI.indentLevel--;
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            EditorGUILayout.Space();

            // INSTANT ACTIONS
            if (_problem.instantActions.Count > 0)
            {
                var instantActions = _problem.instantActions;
                EditorGUILayout.BeginVertical();
                EditorGUILayout.LabelField("INSTANT ACTIONS", EditorStyles.boldLabel);
                EditorGUI.indentLevel++;
                EditorGUILayout.BeginVertical();
                for (var i = 0; i < instantActions.Count; i++)
                {
                    var action = instantActions[i];
                    EditorGUILayout.LabelField(action.ToString(), EditorStyles.whiteBoldLabel);

                    EditorGUI.indentLevel++;
                    EditorGUILayout.LabelField("INSTANT EFFECTS", EditorStyles.boldLabel);
                    EditorGUI.indentLevel++;
                    EditorGUILayout.BeginVertical();
                    foreach (var effect in action.effects)
                    {
                        EditorGUILayout.TextArea(effect.ToString(), EditorStyles.largeLabel);
                    }
                    EditorGUILayout.EndVertical();
                    EditorGUI.indentLevel--;
                    EditorGUI.indentLevel--;
                    EditorGUILayout.Space();
                }
                EditorGUILayout.EndVertical();
                EditorGUI.indentLevel--;
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
                EditorGUILayout.Space();
                EditorGUILayout.EndVertical();
            }

            // DURATIVE ACTIONS
            if (_problem.durativeActions.Count > 0)
            {
                var durativeActions = _problem.durativeActions;
                EditorGUILayout.BeginVertical();
                EditorGUILayout.LabelField("DURATIVE ACTIONS", EditorStyles.boldLabel);

                EditorGUI.indentLevel++;
                EditorGUILayout.BeginVertical();
                for (var i = 0; i < durativeActions.Count; i++)
                {
                    var action = durativeActions[i];
                    EditorGUILayout.LabelField(action.ToString(), EditorStyles.whiteBoldLabel);
                    EditorGUI.indentLevel++;
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Duration: " + action.duration.ToString(), EditorStyles.boldLabel);
                    EditorGUILayout.EndHorizontal();
                    EditorGUI.indentLevel--;


                    EditorGUI.indentLevel++;
                    EditorGUILayout.LabelField("DURATIVE EFFECTS", EditorStyles.boldLabel);
                    EditorGUI.indentLevel++;
                    EditorGUILayout.BeginVertical();
                    foreach (var effect in action.effects)
                    {
                        EditorGUILayout.TextArea(effect.ToString(), EditorStyles.largeLabel);
                    }
                    EditorGUILayout.EndVertical();
                    EditorGUI.indentLevel--;
                    EditorGUI.indentLevel--;
                    EditorGUILayout.Space();
                }
                EditorGUILayout.EndVertical();
                EditorGUI.indentLevel--;
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
                EditorGUILayout.Space();
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndVertical();
        }

        private void DrawNodes(PdSimTypesDeclaration.TypeNode node, int depth = 0)
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