using PDSim.Animation;
using PDSim.Components;
using PDSim.Simulation.Data;
using PDSim.UI;
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


        private Dictionary<string, PdSimSimulationObject> _objects;

        private Dictionary<string, PdAction> _actions;


        private Dictionary<string, PdTypedPredicate> _predicates;
        private Dictionary<string, FluentAnimation> _effectToAnimations;

        private List<GameObject> currentAnimationObjects = new List<GameObject>();

        public PlanListUI _planListUI;

        private bool _simulationRunning = false;
        public bool SimulationRunning
        {
            get { return _simulationRunning; }
        }

        private void Start()
        {
            // Gameobjects References
            _objects = new Dictionary<string, PdSimSimulationObject>();
            var problemObjectsRootObject = GameObject.Find("Problem Objects");
            for (var i = 0; i < problemObjectsRootObject.transform.childCount; i++)
            {
                var child = problemObjectsRootObject.transform.GetChild(i);
                _objects.Add(child.name, child.gameObject.GetComponent<PdSimSimulationObject>());
            }

            // Predicate References
            _predicates = new Dictionary<string, PdTypedPredicate>();
            foreach (var predicate in fluents.fluents)
            {
                _predicates.Add(predicate.name, predicate);
            }

            // Action References
            _actions = new Dictionary<string, PdAction>();
            foreach (var action in actions.actions)
            {
                _actions.Add(action.name, action);
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

            // Plan List UI
            _planListUI.InitializePlanList(plan.actions);
        }

        private IEnumerator<PdBooleanPredicate> EnumerateFluents(List<PdBooleanPredicate> fluents)
        {
            foreach (var fluent in fluents)
            {
                yield return fluent;
            }
        }

        private IEnumerator<PdBooleanPredicate> EnumerateFluents(PdPlanAction action)
        {
            var actionDefinition = _actions[action.name];

            foreach (var fluent in actionDefinition.effects)
            {
                // Map fluents attributes to action attributes
                var attributeMap = fluent.parameterMapping;
                var yieldFluent = new PdBooleanPredicate()
                {
                    name = fluent.name,
                    attributes = new List<string>(),
                    value = fluent.value
                };
                foreach (var index in attributeMap)
                {
                    yieldFluent.attributes.Add(action.parameters[index]);
                }


                yield return yieldFluent;
            }
        }

        public void StartSimulation()
        {
            StartCoroutine(SimulationRoutine());
        }

        public void PauseSimulation()
        {
            Time.timeScale = 0;
        }

        private IEnumerator SimulationRoutine()
        {
            _simulationRunning = true;
            var fluentEnumerator = EnumerateFluents(problem.initialState);
            yield return AnimationMachineLoop(fluentEnumerator);

            // Animate Plan
            for (var i = 0; i < plan.actions.Count; i++)
            {
                _planListUI.HighlightCurrentAction(i);


                var action = plan.actions[i];
                fluentEnumerator = EnumerateFluents(action);

                yield return AnimationMachineLoop(fluentEnumerator);
            }

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
            //Debug.Log("Animation Data count: " + _effectToAnimations[fluent.name].animationData.Count);
            foreach (var animationData in _effectToAnimations[fluent.name].animationData)
            {
                var inputObjects = fluent.attributes.Select(attribute => _objects[attribute]).ToArray();

                if (!fluent.value && !animationData.name.StartsWith("NOT_"))
                    continue;

                animationQueue.Enqueue(new AnimationQueueElement()
                {
                    animationName = animationData.name,
                    objects = fluent.attributes.Select(attribute => _objects[attribute].gameObject).ToArray()
                });
            }
            //Debug.Log("Queue Count: " + animationQueue.Count);
        }



        private IEnumerator AnimationMachineLoop(IEnumerator<PdBooleanPredicate> fluentEnumerator)
        {
            _animationState = AnimationState.None;
            while (_animationState != AnimationState.Finished)
            {
                switch (_animationState)
                {
                    case AnimationState.None:
                        var next = fluentEnumerator.MoveNext();
                        var fluent = fluentEnumerator.Current;
                        // Get the next fluent
                        if (next)
                        {
                            //Debug.Log("Checking Animation for Fluent: " + _effectToAnimations[fluent.name].metaData.name);
                            // Update the queue
                            UpdateQueue(fluent);
                            if (animationQueue.Count > 0)
                                _animationState = AnimationState.Ready;
                        }
                        else
                        {
                            //Debug.Log("No more fluents to animate");
                            _animationState = AnimationState.Finished;
                        }
                        break;
                    case AnimationState.Ready:
                        var animation = animationQueue.Dequeue();
                        TriggerAnimation(animation.animationName, animation.objects);
                        break;
                    case AnimationState.Running:
                        yield return null;
                        break;
                    case AnimationState.End:
                        // animation has ended, if queue is empty, set state to none, else set state to ready
                        if (animationQueue.Count == 0)
                            _animationState = AnimationState.None;
                        else
                            _animationState = AnimationState.Ready;
                        break;
                    case AnimationState.Finished:
                    default:
                        yield return null;
                        break;
                }
                yield return null;

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