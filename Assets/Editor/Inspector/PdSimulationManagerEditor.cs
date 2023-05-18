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


                //TODO: Works! Create static method

                var graph = instance.GetComponent<ScriptMachine>().graph;

                var effectEvent = new ActionEffectEvent()
                {
                    position = new Vector2(-300, -180),
                    ArgumentCount = 2,
                    EffectName = "At(robot, cell)",
                    EffectArguments = new List<string> { "?r - robot", "?c - cell" }
                };

                

                var inputUnit = new GraphInput();
                inputUnit.position = new Vector2(-250, -30);
                var outputUnit = new GraphOutput();
                outputUnit.position = new Vector2(250, -30);

                var el1 = new ValueInputDefinition();
                el1.key = "r - robot";
                el1.type = typeof(GameObject);

                var el2 = new ValueInputDefinition();
                el2.key = "c - cell";
                el2.type = typeof(GameObject);

                var cont = new ControlInputDefinition();
                cont.key = "Entry";
                cont.hideLabel = true;

                var contrOut = new ControlOutputDefinition();
                contrOut.key = "Exit";
                contrOut.hideLabel = true;
                
                var superUnit = new SubgraphUnit();
                superUnit.nest.source = GraphSource.Embed;
                superUnit.nest.embed = new FlowGraph()
                {
                    valueInputDefinitions = { el1, el2 },
                    controlInputDefinitions = { cont },
                    controlOutputDefinitions = { contrOut }
                };
                
                superUnit.nest.graph.title = "Animation Definition";
                superUnit.nest.graph.summary = "Click to edit animation definition";
                superUnit.nest.embed.units.Add(inputUnit);
                superUnit.nest.embed.units.Add(outputUnit);
                
                
                var effectEndEvent = new ActionEffectEndEvent()
                {
                    position = new Vector2(200, -144),
                    EffectName = "At(robot, cell)"
                };


                graph.units.Add(effectEvent);
                graph.units.Add(superUnit);
                graph.units.Add(effectEndEvent);

                var conn = new ControlConnection(effectEvent.controlOutputs[0], superUnit.controlInputs[0]);
                var conn2 = new ControlConnection(superUnit.controlOutputs[0], effectEndEvent.controlInputs[0]);
                graph.controlConnections.Add(conn);
                graph.controlConnections.Add(conn2);

                var conn3 = new ValueConnection(effectEvent.valueOutputs[0], superUnit.valueInputs[0]);
                var conn4 = new ValueConnection(effectEvent.valueOutputs[1], superUnit.valueInputs[1]);
                graph.valueConnections.Add(conn3);
                graph.valueConnections.Add(conn4);
            }
        }
    }
}