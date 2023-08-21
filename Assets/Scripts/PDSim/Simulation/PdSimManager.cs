using PDSim.Components;
using PDSim.Simulation.Data;
using PDSim.Utils;
using PDSim.VisualScripting;
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

        private bool _simulationRunning = false;
        public bool SimulationRunning
        {
            get { return _simulationRunning; }
        }



        public delegate void SimulationInitBlock();
        public event SimulationInitBlock OnSimulationInitBlock;

        public delegate void SimulationActionBlock(string block, int i);
        public event SimulationActionBlock OnSimulationActionBlock;


        public delegate void SimulationStep(string predicate);
        public event SimulationStep OnSimulationStep;

        public delegate void SimulationReady(List<PdPlanAction> actions);
        public event SimulationReady OnSimulationReady;

        public delegate void SimulationFinished();
        public event SimulationFinished OnSimulationFinished;


        // TODO:Change to be a separate script attached to each object
        public delegate void SimulationObjectHovered(string objectName, List<PdBooleanPredicate> fluents);
        public event SimulationObjectHovered OnSimulationObjectHovered;
        public delegate void SimulationObjectUnhovered();
        public event SimulationObjectUnhovered OnSimulationObjectUnhovered;
        private Dictionary<string, Dictionary<string, PdBooleanPredicate>> objectStates;


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

            objectStates = new Dictionary<string, Dictionary<string, PdBooleanPredicate>>();
            foreach (var obj in _objects)
            {
                objectStates.Add(obj.Key, new Dictionary<string, PdBooleanPredicate>());
            }

            OnSimulationReady(plan.actions);
        }

        private IEnumerator<PdBooleanPredicate> EnumerateFluents(List<PdBooleanPredicate> fluents)
        {
            foreach (var fluent in fluents)
            {
                if (fluent.attributes.Count > 0)
                {
                    var objectName = fluent.attributes[0];
                    objectStates[objectName][fluent.name] = fluent;
                }
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

                if (yieldFluent.attributes.Count > 0)
                {
                    var objectName = yieldFluent.attributes[0];
                    objectStates[objectName][yieldFluent.name] = yieldFluent;
                }

                yield return yieldFluent;
            }
        }

        public void StartSimulation()
        {
            StartCoroutine(SimulationRoutine());
        }

        private IEnumerator SimulationRoutine()
        {
            _simulationRunning = true;
            OnSimulationInitBlock();
            var fluentEnumerator = EnumerateFluents(problem.initialState);
            yield return AnimationMachineLoop(fluentEnumerator);

            // Animate Plan
            for (var i = 0; i < plan.actions.Count; i++)
            {
                var action = plan.actions[i];

                OnSimulationActionBlock(action.name, i);

                fluentEnumerator = EnumerateFluents(action);

                yield return AnimationMachineLoop(fluentEnumerator);
            }
            OnSimulationFinished();
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

                var attributes = fluent.attributes;
                // get types of attributes
                var fluentParam = _effectToAnimations[fluent.name].metaData.parameters;
                var attributeTypes = attributes.Select(attribute => _objects[attribute].objectType).ToList();

                // check if types match string
                if (fluentParam.Count == attributeTypes.Count)
                {
                    var match = true;
                    for (var i = 0; i < fluentParam.Count; i++)
                    {
                        var fluentDefinitionType = fluentParam[i].type;
                        var objectType = attributeTypes[i];
                        //Debug.Log("Type: " + fluentDefinitionType + " " + objectType);
                        if (fluentDefinitionType != objectType && !types.typeTree.IsChildOf(objectType, fluentDefinitionType))
                        {
                            match = false;
                            break;
                        }
                    }
                    if (!match)
                        continue;
                }
                else
                {
                    continue;
                }

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
                        // Log objects one line
                        var objectNames = fluentEnumerator.Current.attributes.Aggregate((a, b) => a + ", " + b);
                        var fluentString = fluentEnumerator.Current.name + "(" + objectNames + ")";
                        OnSimulationStep(fluentString);
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

        public void HoverObject(string name)
        {
            var states = objectStates[name].Values.ToList();
            OnSimulationObjectHovered(name, states);
        }

        public void ClearHover()
        {
            OnSimulationObjectUnhovered();
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
                prefabInstance.GetComponent<PdSimSimulationObject>().objectType = type;
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


                var instance = PrefabUtility.InstantiatePrefab(prefab, problemObjectsRootObject.transform) as PdSimSimulationObject;
                instance.gameObject.name = obj.name;
            }
        }
    }
}