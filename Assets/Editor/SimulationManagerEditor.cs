using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SimulationManager)), CanEditMultipleObjects]
public class SimulationManagerEditor : Editor
{
    private SimulationManager _simulationManager;
    
    public override void OnInspectorGUI()
    {
        _simulationManager = (SimulationManager) target;
        base.OnInspectorGUI();
        if (GUILayout.Button("Generate Scene"))
        {
            Debug.Log("Generate scene");
        }
    }
}
