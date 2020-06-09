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

        if (_simulationManager.simulationSettings != null)
        {
            if (GUILayout.Button("Generate Scene"))
            {
                
            }
        }
    }
}