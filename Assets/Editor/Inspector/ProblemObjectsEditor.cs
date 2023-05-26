using PDSim.Simulation;
using UnityEditor;
using UnityEngine;

namespace Editor.Inspector
{
    [CustomEditor(typeof(ProblemObjects))]
    public class ProblemObjectsEditor : UnityEditor.Editor
    {
        private ProblemObjects problemObjects;

        private void OnEnable()
        {
            problemObjects = (ProblemObjects)target;
        }
        public override void OnInspectorGUI()
        {
            GUILayout.Label("Problem Objects Customisation", EditorStyles.largeLabel);
            for (var i = 0; i < problemObjects.prefabs.Count; ++i)
            {
                DrawModel(i);
            }
        }

        private void DrawModel(int index)
        {
            EditorGUILayout.BeginVertical(GUI.skin.box);
            // MODEL NAME
            EditorGUILayout.LabelField(problemObjects.prefabs[index].name, EditorStyles.boldLabel);
            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.ObjectField(problemObjects.prefabs[index], typeof(PdSimSimulationObject), false);
            // EDIT PREFAB BUTTON
            if (GUILayout.Button("Edit", GUILayout.ExpandWidth(false)))
            {
                if (!AssetDatabase.OpenAsset(problemObjects.prefabs[index]))
                {
                    throw new UnityException("Can't Open Prefab");
                }
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
        }
    }
}