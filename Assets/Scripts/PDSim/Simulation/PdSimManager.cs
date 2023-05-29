using PDSim.Simulation.Data;
using PDSim.Utils;
using System.Collections.Generic;
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

            foreach (var leafNode in leafNodes)
            {
                var folderPath = AssetUtils.GetSimulationObjectsPath(SceneManager.GetActiveScene().name);
                // Get the generic object prefab
                Object originalPrefab = (GameObject)AssetDatabase.LoadAssetAtPath("Assets/Prefabs/PDSim/PdSimObject.prefab", typeof(GameObject));
                // Create instance of generic object
                var prefabInstance = PrefabUtility.InstantiatePrefab(originalPrefab, null) as GameObject;
                // Save new model
                var newModel = PrefabUtility.SaveAsPrefabAsset(prefabInstance, folderPath + "/" + leafNode + ".prefab");

                if (problemObjects.prefabs == null)
                {
                    problemObjects.prefabs = new List<PdSimSimulationObject>();
                }
                problemObjects.prefabs.Add(newModel.GetComponent<PdSimSimulationObject>());

                // Remove from scene
                DestroyImmediate(prefabInstance);
            }


            // Spawn objects
            foreach (var obj in problem.objects)
            {
                var type = obj.type;
                var prefabPath = AssetUtils.GetSimulationObjectsPath(SceneManager.GetActiveScene().name) + "/" + type + ".prefab";
                var prefab = AssetDatabase.LoadAssetAtPath<PdSimSimulationObject>(prefabPath);
                var instance = PrefabUtility.InstantiatePrefab(prefab, problemObjectsRootObject.transform) as PdSimSimulationObject;
                instance.name = obj.name;
                prefab.objectType = type;
            }
        }
    }
}