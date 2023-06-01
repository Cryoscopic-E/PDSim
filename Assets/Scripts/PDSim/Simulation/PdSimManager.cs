using PDSim.Simulation.Data;
using PDSim.Utils;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;


namespace PDSim.Simulation
{
    public class PdSimManager : MonoBehaviour
    {
        // Singleton
        private static PdSimManager _instance;
        public static PdSimManager Instance
        {
            get
            {
                if (_instance == null)
                    _instance = FindObjectOfType<PdSimManager>();
                return _instance;
            }
        }



        public CustomTypes types;
        public Fluents fluents;
        public Actions actions;
        public Plan plan;
        public Problem problem;


        private Dictionary<string, GameObject> _objects;

        private void Start()
        {
            _objects = new Dictionary<string, GameObject>();
            var problemObjectsRootObject = GameObject.Find("Problem Objects");
            for (var i = 0; i < problemObjectsRootObject.transform.childCount; i++)
            {
                var child = problemObjectsRootObject.transform.GetChild(i);
                _objects.Add(child.name, child.gameObject);
            }


        }



        private void TriggerAnimation()
        {
            // TODO: Works! Implement this for simulate plan!
            //Debug.Log("ActionEffectEvent: At(robot, cell)");
            //EventBus.Register<string>(EventNames.actionEffectEnd, i =>
            //{
            //    Debug.Log("RECEIVED " + i);
            //});
            //EventBus.Trigger(EventNames.actionEffectStart, new EffectEventArgs("At(robot, cell)", objs.ToArray()));
            throw new System.NotImplementedException();
        }

        public void SetUpAnimations()
        {
            var animationsRootObject = GameObject.Find("Animations");
            if (animationsRootObject == null)
            {
                animationsRootObject = new GameObject("Animations");
            }

            foreach (var fluent in fluents.fluents)
            {
                var fluentAnimation = animationsRootObject.AddComponent<FluentAnimation>();
                fluentAnimation.metaData = fluent;
                fluentAnimation.animationData = new List<FluentAnimationData>();
            }
        }

        public void SetUpObjects()
        {
            var problemObjectsRootObject = GameObject.Find("Problem Objects");
            if (problemObjectsRootObject == null)
            {
                problemObjectsRootObject = new GameObject("Problem Objects");
            }

            var problemObjects = problemObjectsRootObject.GetComponent<ProblemObjects>();
            if (problemObjects == null)
            {
                problemObjects = problemObjectsRootObject.AddComponent<ProblemObjects>();
            }


            var leafNodes = types.typeTree.GetLeafNodes();

            foreach (var type in leafNodes)
            {
                var folderPath = AssetUtils.GetSimulationObjectsPath(SceneManager.GetActiveScene().name);
                // Get the generic object prefab
                Object originalPrefab = (GameObject)AssetDatabase.LoadAssetAtPath("Assets/Prefabs/PDSim/PdSimObject.prefab", typeof(GameObject));
                // Create instance of generic object
                var prefabInstance = PrefabUtility.InstantiatePrefab(originalPrefab, null) as GameObject;
                // Save new model
                var newModel = PrefabUtility.SaveAsPrefabAsset(prefabInstance, folderPath + "/" + type + ".prefab");

                if (problemObjects.prefabs == null)
                {
                    problemObjects.prefabs = new List<PdSimSimulationObject>();
                }
                problemObjects.prefabs.Add(newModel.GetComponent<PdSimSimulationObject>());


                //// Create script
                var macro = (IMacro)ScriptableObject.CreateInstance(typeof(ScriptGraphAsset));
                var macroObject = (Object)macro;
                macro.graph = FlowGraph.WithStartUpdate();

                ScriptMachine flowMachine = newModel.AddComponent<ScriptMachine>();

                flowMachine.nest.macro = (ScriptGraphAsset)macro;

                flowMachine.graph.title = "PDSim Object Script";
                flowMachine.graph.summary = "Start/Update Custom Routines";


                var path = AssetUtils.GetSimulationObjectsPath(SceneManager.GetActiveScene().name) + type + ".asset";
                AssetDatabase.CreateAsset(macroObject, path);

                // Remove from scene
                DestroyImmediate(prefabInstance);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            // Spawn objects
            foreach (var obj in problem.objects)
            {
                var type = obj.type;
                var prefabPath = AssetUtils.GetSimulationObjectsPath(SceneManager.GetActiveScene().name) + "/" + type + ".prefab";
                var prefab = AssetDatabase.LoadAssetAtPath<PdSimSimulationObject>(prefabPath);
                prefab.objectType = type;

                var instance = PrefabUtility.InstantiatePrefab(prefab, problemObjectsRootObject.transform) as PdSimSimulationObject;
                instance.gameObject.name = obj.name;
            }
        }
    }
}