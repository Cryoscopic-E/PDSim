using PDSim.Simulation;
using UnityEditor;
using UnityEngine;

namespace Editor.Inspector
{
    [CustomEditor(typeof(PdSimManager))]
    public class PdSimulationManagerEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            if (GUILayout.Button("Create Cube Object"))
            {
                // Load the prefab
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/PDSim/PdSimObject.prefab");
                // Instantiate the prefab as variant
                var instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
                // Set the name of the instance
                instance.name = "Cube";
                // Set the position of the instance
                instance.transform.position = Vector3.zero;
            }
        }
    }
}