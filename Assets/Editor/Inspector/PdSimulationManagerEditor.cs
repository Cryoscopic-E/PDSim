using PDSim.Simulation;
using UnityEditor;

namespace Editor.Inspector
{
    [CustomEditor(typeof(PdSimManager))]
    public class PdSimulationManagerEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            // TODO: custom inspector for the simulation manager
        }
    }
}