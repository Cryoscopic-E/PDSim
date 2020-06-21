using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(SimulationEnvironment))]
public class SimulationEnvironmentEditor : Editor
{
    private SimulationEnvironment simulationEnvironment;

    public override void OnInspectorGUI()
    {
        simulationEnvironment = (SimulationEnvironment)target;
        DrawProblemField();
        GUILayout.Space(10);
        if (simulationEnvironment.problem != null)
        {

        }
    }

    private void DrawProblemField()
    {
        GUILayout.Label("Problem File", EditorStyles.boldLabel);
        GUILayout.Space(5);
        EditorGUILayout.BeginHorizontal();
        {
            EditorGUI.BeginChangeCheck();


            TextAsset problem =
                (TextAsset)EditorGUILayout.ObjectField(simulationEnvironment.problem, typeof(TextAsset), true);

            if (GUILayout.Button("Clear"))
            {
                problem = null;
            }

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(simulationEnvironment, "Problem File Change");
                simulationEnvironment.problem = problem;
                if (problem == null)
                {
                    simulationEnvironment.problemElements = null;
                    simulationEnvironment.plan = null;
                    simulationEnvironment.objectsPositions = null;
                }
                else
                {
                    // parse elements
                    simulationEnvironment.problemElements = Parser.ParseProblem(simulationEnvironment.problem.text); //, simulationSettings.problem.text);
                    simulationEnvironment.objectsPositions = new Vector3[simulationEnvironment.problemElements.objects.Count];
                }

                // save asset
                EditorUtility.SetDirty(simulationEnvironment);
            }
        }
        EditorGUILayout.EndHorizontal();
    }
}
