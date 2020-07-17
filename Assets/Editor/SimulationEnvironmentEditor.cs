using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    [CustomEditor(typeof(SimulationEnvironment))]
    public class SimulationEnvironmentEditor : UnityEditor.Editor
    {
        private SimulationEnvironment _simulationEnvironment;

        public override void OnInspectorGUI()
        {
            _simulationEnvironment = (SimulationEnvironment)target;
            DrawProblemField();
            GUILayout.Space(10);
        }

        private void DrawProblemField()
        {
            // ==========================
            // ======== PROBLEM =========
            // ==========================
        
            // Check if changed
            EditorGUI.BeginChangeCheck();
            // Problem File 
            EditorGUILayout.BeginVertical();
            GUILayout.Label("Problem File", EditorStyles.boldLabel);
            GUILayout.Space(5);
            // Draw domain filed
            EditorGUILayout.BeginHorizontal();
            var problem = (TextAsset) EditorGUILayout.ObjectField(_simulationEnvironment.problem, typeof(TextAsset), true);
            if (GUILayout.Button("Clear"))
            {
                problem = null;
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(_simulationEnvironment, "Problem File Change");
                _simulationEnvironment.problem = problem;

                if (problem != null)
                    _simulationEnvironment.Initialize();
                else
                    _simulationEnvironment.Reset();
                // save asset
                EditorUtility.SetDirty(_simulationEnvironment);
            }
        }
    }
}
