using UnityEditor;
using UnityEngine;

namespace Editor
{
    [CustomEditor(typeof(SimulationSettings))]
    public class SimulationSettingsEditor : UnityEditor.Editor
    {
        private SimulationSettings _simulationSettings;

        public override void OnInspectorGUI()
        {
            _simulationSettings = (SimulationSettings) target;
            GUILayout.Space(10);
            DrawDomainField();

            if (_simulationSettings.domain != null)
            {
                GUILayout.Space(15);
                DrawPredicates();
            }
        }

        private void DrawDomainField()
        {
            // =========================
            // ======== DOMAIN =========
            // =========================
        
            // Check if changed
            EditorGUI.BeginChangeCheck();
            // Domain File 
            EditorGUILayout.BeginVertical();
            GUILayout.Label("Domain File", EditorStyles.boldLabel);
            GUILayout.Space(5);
            // Draw domain filed
            EditorGUILayout.BeginHorizontal();
            var domain = (TextAsset) EditorGUILayout.ObjectField(_simulationSettings.domain, typeof(TextAsset), true);
            if (GUILayout.Button("Clear"))
            {
                domain = null;
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(_simulationSettings, "Domain File Change");
                _simulationSettings.domain = domain;

                if (domain != null)
                {
                    // parse elements
                    Parser.ParseDomain(
                        _simulationSettings.domain.text,
                        out _simulationSettings.predicates,
                        out  _simulationSettings.actions);
                    // instantiate predicates behaviour array
                    _simulationSettings.predicatesBehaviours = new PredicateCommand[_simulationSettings.predicates.Count];
                }
                // save asset
                EditorUtility.SetDirty(_simulationSettings);
            }
            // ==============================
            // CHECK FILES AFTER MODIFICATION
            // ==============================
            if (domain != null) return;
            _simulationSettings.actions = null;
            _simulationSettings.predicates = null;
            _simulationSettings.predicatesBehaviours = null;
        }

        private void DrawPredicates()
        {
            EditorGUILayout.BeginVertical();
            {
                GUILayout.Label("PDDL Predicates", EditorStyles.boldLabel);
            
                for (int i = 0; i < _simulationSettings.predicates.Count; i++)
                {
                    DrawBehavioursInput(i);
                }
            }
            EditorGUILayout.EndVertical();
        }

        private void DrawBehavioursInput(int index)
        {
            // DRAW LABEL OF TYPE NAME
            GUILayout.BeginHorizontal();
            GUILayout.Label(_simulationSettings.predicates[index].name.ToUpper(), EditorStyles.largeLabel);
        
            EditorGUI.BeginChangeCheck();
            // GET BEHAVIOUR FIELD
            var behavior = (PredicateCommand)EditorGUILayout.ObjectField(_simulationSettings.predicatesBehaviours[index], typeof(PredicateCommand),
                false, GUILayout.ExpandWidth(false));

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(_simulationSettings, "Predicate Behaviour Set");
                _simulationSettings.predicatesBehaviours[index] = behavior;
                EditorUtility.SetDirty(this);
            }
        
            GUILayout.EndHorizontal();
        }
    }
}