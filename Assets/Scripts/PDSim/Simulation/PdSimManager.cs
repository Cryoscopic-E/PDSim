using PDSim.Protobuf;
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
        // Singleton Instance
        // ------------------

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

        // Data Assets (Problem model, plan, etc.)
        // ---------------------------------------

        public PdSimProblem problemModel;
        public PdSimInstance problemInstance;

        private bool isTimedProblem;


        // Dicionaries for accessing data runtime
        // --------------------------------------

        // Access GameObjects with problem objects' names
        private Dictionary<string, PdSimSimulationObject> _objects;

        // Access problem objects names with type
        private Dictionary<string, List<string>> _typeToObjects;

        // Access Instantaneous actions with action names
        private Dictionary<string, PdSimInstantaneousAction> _instantaneousActions;

        // Access Durative actions with action names
        private Dictionary<string, PdSimDurativeAction> _durativeActions;

        // Access Fluents with fluent names
        private Dictionary<string, PdSimFluent> _predicates;

        // Map fluent effect names to animations
        private Dictionary<string, FluentAnimation> _effectToAnimations;


        // Simulation Data
        // ---------------

        // Current environment state
        private Dictionary<string, PdSimFluentAssignment> environmentState = new Dictionary<string, PdSimFluentAssignment>();

        // Current animation objects active
        private List<GameObject> currentAnimationObjects = new List<GameObject>();

        // Simulation State
        private bool _simulationRunning = false;
        public bool SimulationRunning
        {
            get { return _simulationRunning; }
        }


        // Events and Delegates
        // --------------------

        // OnTemporalSimulation set all subscribed elements to display temporal planning
        public delegate void TemporalSimulation();
        public event TemporalSimulation OnTemporalSimulation;

        // OnSimulationInitBlock is called when the simulation is animating the initial state
        public delegate void SimulationInitBlock();
        public event SimulationInitBlock OnSimulationInitBlock;

        // OnSimulationActionBlock is called when the simulation is animating an action
        public delegate void SimulationActionBlock(string block, int i);
        public event SimulationActionBlock OnSimulationActionBlock;

        // OnSimulationStep is called when the simulation is animating a single fluent
        public delegate void SimulationStep(string predicate);
        public event SimulationStep OnSimulationStep;

        // OnSimulationReady is called when the simulation is ready to start
        public delegate void SimulationReady(List<PdSimActionInstance> actions);
        public event SimulationReady OnSimulationReady;

        // OnSimulationFinished is called when the simulation is finished
        public delegate void SimulationFinished();
        public event SimulationFinished OnSimulationFinished;

        // OnSimulationObjectHovered is called when the mouse hovers over an object
        public delegate void SimulationObjectHovered(PdSimSimulationObject simulationObject);
        public event SimulationObjectHovered OnSimulationObjectHovered;

        // OnSimulationObjectUnHovered is called when the mouse exits an object
        public delegate void SimulationObjectUnhovered();
        public event SimulationObjectUnhovered OnSimulationObjectUnhovered;


        private void Start()
        {
            isTimedProblem = problemModel.durativeActions.Count > 0;
            if (isTimedProblem)
                OnTemporalSimulation();

            // Gameobjects References
            _objects = new Dictionary<string, PdSimSimulationObject>();
            var problemObjectsRootObject = GameObject.Find("Problem Objects");
            for (var i = 0; i < problemObjectsRootObject.transform.childCount; i++)
            {
                var child = problemObjectsRootObject.transform.GetChild(i);
                _objects.Add(child.name, child.gameObject.GetComponent<PdSimSimulationObject>());
            }

            var fluents = problemModel.fluents;
            // Predicate References
            _predicates = new Dictionary<string, PdSimFluent>();
            foreach (var predicate in fluents)
            {
                _predicates.Add(predicate.name, predicate);
            }

            var actions = problemModel.instantaneousActions;
            // Action References
            _instantaneousActions = new Dictionary<string, PdSimInstantaneousAction>();
            foreach (var action in actions)
            {
                _instantaneousActions.Add(action.name, action);
            }

            var durativeActions = problemModel.durativeActions;
            // Durative Action References
            _durativeActions = new Dictionary<string, PdSimDurativeAction>();
            foreach (var action in durativeActions)
            {
                _durativeActions.Add(action.name, action);
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
        }

        private IEnumerator<PdSimFluentAssignment> EnumerateFluentAssignments(List<PdSimFluentAssignment> fluents)
        {
            foreach (var fluent in fluents)
            {
                // IMPORTANT: Assumption that the first parameter is the object name (e.g. (at ?o ?l))
                // ?o is the object which state is being changed
                if (fluent.parameters.Count > 0)
                {
                    var objectName = fluent.parameters[0];
                    var obj = _objects[objectName];
                    obj.AddFluentAssignment(fluent);
                }
                else
                {
                    environmentState[fluent.fluentName] = fluent;
                }

                yield return fluent;
            }
        }

        private IEnumerator<PdSimFluentAssignment> EnumerateActionEffects(PdSimActionInstance planAction, List<PdSimEffect> pdSimActionEffect)
        {
            foreach (var effect in pdSimActionEffect)
            {
                switch (effect.effectKind)
                {
                    default:
                    case EffetKind.None:
                        break;
                    case EffetKind.Assignment:
                        break;
                    case EffetKind.Increase:
                        break;
                    case EffetKind.Decrease:
                        break;
                }






                var parametersMap = effect.actionParametersMap;
                var actionPlanParameters = planAction.parameters;

                var parametersObjects = new List<string>();
                foreach (var p in parametersMap)
                {
                    parametersObjects.Add(actionPlanParameters[p]);
                }




            }
            yield return null;
        }


        // private List<PdSimFluentAssignment> EffectsFromActionInstance(PdSimActionInstance actionInstance)
        // {
        //    var effects = new List<PdSimFluentAssignment>();
        //    var actionName = actionInstance.name;

        //    // Get the action from the instantanous actions or durative actions
        //    PdSimInstantaneousAction actionInst = null;
        //    PdSimDurativeAction actionDur = null;

        //    if (_instantaneousActions.ContainsKey(actionName))
        //    {
        //        actionInst = _instantaneousActions[actionName];
        //        var list = new List<PdSimFluentAssignment>();
        //        foreach (var effect in actionInst.effects)
        //        {
        //            list.Add(effect.fluentAssignment);
        //        }
        //        return list;
        //    }
        //    else if (_durativeActions.ContainsKey(actionName))
        //    {
        //        actionDur = _durativeActions[actionName];
        //    }
        //    else
        //    {
        //        Debug.LogError("Action not found: " + actionName);
        //        return effects;
        //    }


        //    return effects;
        // }

        public void StartSimulation()
        {
            StartCoroutine(SimulationRoutine());
        }


        private IEnumerator SimulateSequentialPlan()
        {
            var plan = problemInstance.plan;
            for (var i = 0; i < plan.Count; i++)
            {
                var planAction = plan[i];

                OnSimulationActionBlock(planAction.name, i);

                var actionDefinition = _instantaneousActions[planAction.name];

                var fluentEnumerator = EnumerateActionEffects(planAction, actionDefinition.effects);

                yield return AnimationMachineLoop(fluentEnumerator);
            }
            OnSimulationFinished();
            yield return null;
        }

        private IEnumerator SimulateTemporalPlan()
        {
            yield return null;
        }

        private IEnumerator SimulationRoutine()
        {
            _simulationRunning = true;
            OnSimulationInitBlock();

            var fluentEnumerator = EnumerateFluentAssignments(problemInstance.init);

            yield return AnimationMachineLoop(fluentEnumerator);

            if (isTimedProblem)
            {
                yield return SimulateTemporalPlan();
            }
            else
            {
                yield return SimulateSequentialPlan();
            }

            OnSimulationFinished();
            yield return null;
        }





        // Animation
        // ---------

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

        private void UpdateQueue(PdSimFluentAssignment fluent)
        {
            //Debug.Log("Animation Data count: " + _effectToAnimations[fluent.name].animationData.Count);
            foreach (var animationData in _effectToAnimations[fluent.fluentName].animationData)
            {
                var inputObjects = fluent.parameters.Select(p => _objects[p]).ToArray();


                var fluentValueContent = fluent.value.contentCase;
                // Case 1: Fluent is Boolean type
                if (fluentValueContent == Atom.ContentOneofCase.Boolean)
                {
                    // check fluent value and animation name to see if they match (e.g. (at ?o ?l) and at_object_location) or not((at ?o ?l)) and NOT_at_object_location))
                    if (!fluent.value.IsTrue() && !animationData.name.StartsWith("NOT_"))
                        continue;


                    var attributes = fluent.parameters;
                    var fluentParam = _effectToAnimations[fluent.fluentName].metaData.parameters;
                    var attributeTypes = attributes.Select(attribute => _objects[attribute].objectType).ToList();
                    var typeTree = problemModel.typesDeclaration;
                    // check if types match string
                    if (fluentParam.Count == attributeTypes.Count)
                    {
                        var match = true;
                        for (var i = 0; i < fluentParam.Count; i++)
                        {
                            var fluentDefinitionType = fluentParam[i].type;
                            var objectType = attributeTypes[i];
                            //Debug.Log("Type: " + fluentDefinitionType + " " + objectType);
                            if (fluentDefinitionType != objectType && !typeTree.IsChildOf(objectType, fluentDefinitionType))
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
                        objects = fluent.parameters.Select(p => _objects[p].gameObject).ToArray()
                    });
                }
            }
            //Debug.Log("Queue Count: " + animationQueue.Count);
        }

        private IEnumerator AnimationMachineLoop(IEnumerator<PdSimFluentAssignment> fluentEnumerator)
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
                        var objectNames = fluentEnumerator.Current.parameters.Aggregate((a, b) => a + ", " + b);
                        var fluentString = fluentEnumerator.Current.fluentName + "(" + objectNames + ")";
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

        public void HoverObject(PdSimSimulationObject simulationObject)
        {
            OnSimulationObjectHovered(simulationObject);
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

            foreach (var fluent in problemModel.fluents)
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

            var leafNodes = problemModel.typesDeclaration.GetLeafNodes();
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
            foreach (var obj in problemInstance.objects)
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