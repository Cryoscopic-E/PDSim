using PDSim.Simulation;
using PDSim.Animation;
using System.Collections.Generic;
using Unity.VisualScripting;
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

            if (GUILayout.Button("Setup Animations"))
            {
                var pdSimManager = target as PdSimManager;
                pdSimManager.SetUpAnimations();
            }

            if (GUILayout.Button("Create Cube Object"))
            {
                // Load the prefab
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/PDSim/PdSimObject.prefab");
                // Instantiate the prefab as variant
                var instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
                PrefabUtility.UnpackPrefabInstance(instance, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
                // Set the name of the instance
                instance.name = "Cube";
                // Set the position of the instance
                instance.transform.position = Vector3.zero;
            }
        }
    }
}