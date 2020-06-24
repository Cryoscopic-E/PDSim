using UnityEditor;
using UnityEngine;

namespace Editor
{
    [CustomEditor(typeof(SimulationManager)), CanEditMultipleObjects]
    public class SimulationManagerEditor : UnityEditor.Editor
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
                    //_simulationManager.Plan();
                    _simulationManager.GenerateScene();
                }

                if (GUILayout.Button("Save Environment"))
                {
                    _simulationManager.SaveEnvironment();
                }
            }
        }
    }
}