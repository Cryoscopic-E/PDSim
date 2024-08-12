using PDSim.Protobuf;
using PDSim.Utils;
using PDSim.VisualScripting.Events;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PDSim.Simulation
{

    public struct SimulationState
    {
        public bool isTimedProblem;
        public bool simulatingPlan;
        public PdSimActionInstance currentAction;
    }

    public class PdSimManager : MonoBehaviour
    {

        // State of the simulation
        private SimulationState _simulationState;

        public SimulationState SimulationState
        {
            get { return _simulationState; }
        }

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

        // State
        // -----
        private State _state;


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
            if (problemModel == null)
            {
                return;
            }

            // Problem is temporal set UI
            isTimedProblem = problemModel.durativeActions.Count > 0;
            if (isTimedProblem)
                OnTemporalSimulation();

            // State
            _state = new State();
            foreach (var init in problemInstance.init)
            {
                var node = StateNode.FromAssignment(init);
                _state.AddOrUpdate(node);
            }

            // Simulation State
            _simulationState = new SimulationState()
            {
                isTimedProblem = isTimedProblem,
                simulatingPlan = false,
                currentAction = null
            };

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

            // Type to objects name map
            _typeToObjects = new Dictionary<string, List<string>>();
            foreach (var obj in problemInstance.objects)
            {
                if (!_typeToObjects.ContainsKey(obj.type))
                    _typeToObjects.Add(obj.type, new List<string>());
                _typeToObjects[obj.type].Add(obj.name);
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
            var effectsAnimationsRootObject = GameObject.Find("Effects Animations");
            var fluentAnimations = effectsAnimationsRootObject.GetComponents<FluentAnimation>();

            _effectToAnimations = new Dictionary<string, FluentAnimation>();
            foreach (var fluentAnimation in fluentAnimations)
            {
                // Effect needs to match the name of the predicate
                _effectToAnimations.Add(fluentAnimation.metaData.name, fluentAnimation);
            }


            OnSimulationReady(problemInstance.plan);
        }

        /// <summary>
        /// Given a list of grounded fluents, update the state and the objects
        /// Yield return each fluent
        /// </summary>
        /// <param name="groundedFluents">Grounded action effects</param>
        /// <returns>Fluent effect that have been added to the world state</returns>
        private IEnumerator<PdSimFluentAssignment> EnumerateFluentAssignments(List<PdSimFluentAssignment> groundedFluents)
        {
            foreach (var fluent in groundedFluents)
            {
                // Update object state

                // IMPORTANT: Assumption that the first parameter is the object name (e.g. (at ?o ?l))
                // ?o is the object which state is being changed e.g. (at ?o ?l) -> ?o at ?l
                if (fluent.parameters.Count > 0)
                {
                    var objectName = fluent.parameters[0];
                    var obj = _objects[objectName];
                    obj.AddFluentAssignment(fluent);
                }

                // Update world state
                _state.AddOrUpdate(fluent);
                yield return fluent;
            }
        }

        /// <summary>
        /// Given a action instance (as appears in the plan) and a effect from the action definition
        /// Ground the effect with the parameters of the action instance
        /// </summary>
        /// <param name="planAction">Plan action e.g: move(r1,l0,l1)</param>
        /// <param name="effect">Action Effect</param>
        /// <returns>Grounded Effect</returns>
        private PdSimFluentAssignment GroundEffect(PdSimActionInstance planAction, PdSimEffect effect)
        {
            var parametersMap = effect.actionParametersMap;
            var effectFluent = effect.fluentAssignment;
            var actionPlanParameters = planAction.parameters;
            var objectsParameters = new List<string>();

            for (var i = 0; i < parametersMap.Count; i++)
            {
                var p = parametersMap[i];
                if (p == -1)
                    objectsParameters.Add(effectFluent.parameters[i]);
                else
                {
                    var parameter = actionPlanParameters[p];
                    objectsParameters.Add(parameter);
                }
            }

            var valueApplied = new PdSimAtom(effectFluent.value);

            switch (effect.effectKind)
            {
                default:
                case EffetKind.None:
                    break;
                case EffetKind.Assignment:
                    if (valueApplied.contentCase == Atom.ContentOneofCase.Symbol)
                    {
                        // check if the symbol is a parameter of the action
                        var symbol = valueApplied.valueSymbol;
                        // TODO: This is a hack, move to the model
                        var actionDefinition = _instantaneousActions[planAction.name];
                        var parameterIndex = actionDefinition.parameters.FindIndex(p => p.name == symbol);

                        if (parameterIndex != -1)
                        {
                            // if it is, get the value of the parameter
                            var parameterValue = planAction.parameters[parameterIndex];
                            valueApplied.valueSymbol = parameterValue;
                        }

                    }
                    break;
                case EffetKind.Decrease:
                    valueApplied.DecreaseValue(float.Parse(valueApplied.valueSymbol));
                    break;
                case EffetKind.Increase:
                    valueApplied.IncreaseValue(float.Parse(valueApplied.valueSymbol));
                    break;
            }

            return new PdSimFluentAssignment(valueApplied, effectFluent.fluentName, objectsParameters);
        }


        /// <summary>
        /// Enumerate the grounded effects of an action instance
        /// </summary>
        /// <param name="planAction">Plan action e.g: move(r1,l0,l1)</param>
        /// <param name="pdSimActionEffect">Action Effect definition</param>
        /// <returns>Enumerator of the grounded effects</returns>
        private IEnumerator<PdSimFluentAssignment> EnumerateActionEffects(PdSimActionInstance planAction, List<PdSimEffect> actionEffects)
        {

            var fluentsEffect = new List<PdSimFluentAssignment>();

            foreach (var effect in actionEffects)
            {

                var groundedEffect = GroundEffect(planAction, effect);

                fluentsEffect.Add(groundedEffect);
            }
            return EnumerateFluentAssignments(fluentsEffect);
        }

        public void StartSimulation()
        {
            StartCoroutine(SimulationRoutine());
        }

        public PdSimActionInstance CurrentAction { get; set; }

        private IEnumerator SimulateSequentialPlan()
        {
            _simulationState.simulatingPlan = true;

            var plan = problemInstance.plan;
            for (var i = 0; i < plan.Count; i++)
            {
                var planAction = plan[i];

                _simulationState.currentAction = planAction;

                OnSimulationActionBlock(planAction.name, i);

                var actionDefinition = _instantaneousActions[planAction.name];

                var fluentEnumerator = EnumerateActionEffects(planAction, actionDefinition.effects);

                yield return AnimationMachineLoop(fluentEnumerator);
            }
            OnSimulationFinished();

            _simulationState.simulatingPlan = false;
            _simulationState.currentAction = null;

            yield return null;
        }

        private float ActionTime(float time)
        {
            return (float)System.Math.Round(time, 2);
        }

        private IEnumerator SimulateTemporalPlan()
        {
            OnTemporalSimulation();

            var plan = problemInstance.plan;
            // Create a dictionary of plan actions with the time they start as key
            Dictionary<float, List<PdSimActionInstance>> planBlocks = new Dictionary<float, List<PdSimActionInstance>>();
            // Populate the dictionary with all the action start times
            foreach (var action in plan)
            {
                var startTime = action.startTime;
                startTime = ActionTime(startTime);
                if (!planBlocks.ContainsKey(startTime))
                    planBlocks.Add(startTime, new List<PdSimActionInstance>());
                planBlocks[startTime].Add(action);
            }

            // Sort the dictionary by key (time)
            var sortedPlanBlocks = planBlocks.OrderBy(x => x.Key).ToList();

            // For each block of actions
            for (var i = 0; i < sortedPlanBlocks.Count; i++)
            {
                var block = sortedPlanBlocks[i];
                var startTime = block.Key;
                var actions = block.Value;

                OnSimulationActionBlock("Block " + i, i);

                // For each action in the block
                for (var j = 0; j < actions.Count; j++)
                {
                    var action = actions[j];
                    var actionDefinition = _durativeActions[action.name];


                }
            }

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

            public PdSimAtom value;

            public GameObject[] objects;
        }

        private Queue<AnimationQueueElement> animationQueue = new Queue<AnimationQueueElement>();

        private void UpdateQueue(PdSimFluentAssignment fluent)
        {
            // Check if the fluent has an animation
            foreach (var animationData in _effectToAnimations[fluent.fluentName].animationData)
            {

                if (fluent.value.IsEmpty())
                    continue;

                var inputObjects = fluent.parameters.Select(p => _objects[p]).ToArray();

                var attributes = fluent.parameters;
                var fluentParam = _effectToAnimations[fluent.fluentName].metaData.parameters;
                var attributeTypes = attributes.Select(attribute => _objects[attribute].objectType).ToList();
                var typeTree = problemModel.typesDeclaration;

                if (fluentParam.Count != attributeTypes.Count)
                    continue;
                else
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

                animationQueue.Enqueue(new AnimationQueueElement()
                {
                    animationName = animationData.name,
                    value = fluent.value,
                    objects = fluent.parameters.Select(p => _objects[p].gameObject).ToArray()
                });
            }
        }

        // Animation Machine
        // -----------------

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
                        TriggerAnimation(animation.animationName, animation.value, animation.objects);
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

        private void TriggerAnimation(string animationName, PdSimAtom value, GameObject[] objects)
        {
            _animationState = AnimationState.Running;
            EventBus.Register<string>(EventNames.actionEffectEnd, AnimationEndHandler);

            EventBus.Trigger(EventNames.actionEffectStart, new EffectEventArgs(animationName, value, objects));
        }

        // Mouse Hover objects events
        // --------------------------
        public void HoverObject(PdSimSimulationObject simulationObject)
        {
            OnSimulationObjectHovered(simulationObject);
        }

        public void ClearHover()
        {
            OnSimulationObjectUnhovered();
        }

        // Scene Setup
        // -----------

        // Add the animations to the scene
        public void SetUpAnimations()
        {
            var animationsRootObject = GameObject.Find("Effects Animations");
            if (animationsRootObject == null)
            {
                animationsRootObject = new GameObject("Effects Animations");
            }

            foreach (var fluent in problemModel.fluents)
            {
                var fluentAnimation = animationsRootObject.AddComponent<FluentAnimation>();
                fluentAnimation.metaData = fluent;
                fluentAnimation.animationData = new List<FluentAnimationData>();
            }
        }

        # if UNITY_EDITOR
        // Add the problem objects to the scene
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
                Object originalPrefab = (GameObject)AssetDatabase.LoadAssetAtPath(CommonPaths.PDSIM_OBJECT_PREFAB, typeof(GameObject));
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
        # endif

        // Helper Functions
        // ----------------

        public PdSimSimulationObject GetSimulationObject(string name)
        {
            return _objects[name];
        }
    }
}