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

            if (GUILayout.Button("Create Animation"))
            {
                // Load the prefab
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/PDSim/Fluent Animation.prefab");
                // Instantiate the prefab as variant
                var instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
                PrefabUtility.UnpackPrefabInstance(instance, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
                // Set the name of the instance
                instance.name = "Animation";
                // Set the position of the instance
                instance.transform.position = Vector3.zero;
                

                // Set graph
                var graph = instance.GetComponent<ScriptMachine>().graph;

                var effectEvent = new ActionEffectEvent()
                {
                    position = new Vector2(-204, -144),
                    ArgumentCount = 1,
                    EffectName = "Not-At",
                    EffectArguments =new List<string> {"?c - cell" }
                };
                graph.units.Add(effectEvent);
                
            }
        }
    }
}