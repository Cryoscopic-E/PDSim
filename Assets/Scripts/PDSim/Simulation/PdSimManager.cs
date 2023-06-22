using PDSim.Animation;
using PDSim.Components;
using PDSim.Simulation.Data;
using PDSim.Utils;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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


        private Dictionary<string, PdTypedPredicate> _predicates;
        private Dictionary<string, List<PdBooleanPredicate>> _actionToEffects;
        private Dictionary<string, FluentAnimation> _effectToAnimations;

        private List<GameObject> currentAnimationObjects = new List<GameObject>();

        private void Start()
        {
            // Gameobjects References
            _objects = new Dictionary<string, GameObject>();
            var problemObjectsRootObject = GameObject.Find("Problem Objects");
            for (var i = 0; i < problemObjectsRootObject.transform.childCount; i++)
            {
                var child = problemObjectsRootObject.transform.GetChild(i);
                _objects.Add(child.name, child.gameObject);
            }

            // Predicate References
            _predicates = new Dictionary<string, PdTypedPredicate>();
            foreach (var predicate in fluents.fluents)
            {
                _predicates.Add(predicate.name, predicate);
            }

            // Action to Effects References
            _actionToEffects = new Dictionary<string, List<PdBooleanPredicate>>();
            foreach (var action in actions.actions)
            {
                _actionToEffects.Add(action.name, action.effects);
            }


            // Effect Animation References
            var animationsRootObject = GameObject.Find("Animations");
            var fluentAnimations = animationsRootObject.GetComponents<FluentAnimation>();

            _effectToAnimations = new Dictionary<string, FluentAnimation>();
            foreach (var fluentAnimation in fluentAnimations)
            {
                // Effect needs to match the name of the predicate
                _effectToAnimations.Add(fluentAnimation.metaData.name, fluentAnimation);
            }


            PDSimInit();
            // Start simulation
            //foreach (var action in plan.actions)
            //{
            //    // Get the effects of the action
            //    var effects = _actionToEffects[action.name];

            //    foreach (var effect in effects)
            //    {
            //        // Animation
            //        var animation = _effectToAnimations[effect.name];
            //        if (animation.animationData.Count > 0)
            //            Debug.Log("Animation Defined: " + animation.metaData.name);
            //    }
            //}
        }


        // state machine for the animation of a fluent (start, end)
        public enum AnimationState
        {
            None,
            Ready,
            Running,
            End,
            Finished
        }

        private AnimationState _animationState = AnimationState.None;

        private class AnimationQueueElement
        {
            public string animationName;
            public GameObject[] objects;
        }

        private Queue<AnimationQueueElement> animationQueue = new Queue<AnimationQueueElement>();

        private void UpdateQueue(PdBooleanPredicate fluent)
        {
            foreach (var animationData in _effectToAnimations[fluent.name].animationData)
                {
                    animationQueue.Enqueue(new AnimationQueueElement()
                    {
                        animationName = animationData.name,
                        objects = fluent.attributes.Select(attribute => _objects[attribute]).ToArray()
                    });
                }
        }

        private IEnumerator AnimationMachineLoop(List<PdBooleanPredicate> fluents)
        {
            _animationState = AnimationState.None;
            while (_animationState != AnimationState.Finished)
            {
                for (var fluent in fluents)
                {
                    switch (_animationState)
                    {
                        case AnimationState.None:
                            // animation queue empty check if it can be populated
                            break;
                        case AnimationState.Ready:
                            TriggerAnimation(animationQueue.Dequeue().animationName, animationQueue.Dequeue().objects);
                            break;
                        case AnimationState.Running:
                            yield return null;
                            break;
                        case AnimationState.End:
                            // animation has ended, if queue is empty, set state to none, else set state to ready
                            break;
                        case AnimationState.Finished:
                        default:
                            // animation has finished end coroutine
                            break;
                    }
                }
            }

           
        }


        private void PDSimInit()
        {
            foreach (var fluent in problem.initialState)
            {
                if (_effectToAnimations[fluent.name].animationData.Count > 0)
                {
                    Debug.Log("Animation Defined: " + _effectToAnimations[fluent.name].metaData.name);

                    foreach (var obj in fluent.attributes)
                    {
                        currentAnimationObjects.Add(_objects[obj]);
                    }

                    foreach (var animationData in _effectToAnimations[fluent.name].animationData)
                    {
                        TriggerAnimation(animationData.name);
                    }

                    currentAnimationObjects.Clear();
                }
            }
        }

        private void AnimationEndHandler(string animationName)
        {
            _animationState = AnimationState.End;
        }


        private void TriggerAnimation(string animationName, GameObject[] objects)
        {
            _animationState = AnimationState.Running;
            EventBus.Register<string>(EventNames.actionEffectEnd, AnimationEndHandler);
            EventBus.Trigger(EventNames.actionEffectStart, new EffectEventArgs(animationName, objects));
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