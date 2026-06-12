using GeTPlan.Core.Models;
using GeTPlan.Core.Models.Expressions;
using PDSimAPI;
using PDSim.ScriptableObjects;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PDSim.Components
{
    /// <summary>
    /// Core singleton controller for PDSim. Manages the visualization lifecycle, 
    /// problem and plan data, and coordinates between various components.
    /// </summary>
    public class Controller : MonoBehaviour
    {
        #region Public API

        /// <summary>
        /// Singleton instance of the PDSim Controller.
        /// </summary>
        public static Controller Instance
        {
            get
            {
                if (_instance == null)
                    _instance = FindAnyObjectByType<Controller>();
                return _instance;
            }
        }

        /// <summary>
        /// The planning problem data asset.
        /// </summary>
        [Header("Data Assets")]
        [Tooltip("The planning problem definition.")]
        public ParsedProblem Problem;

        /// <summary>
        /// The plan generation data asset.
        /// </summary>
        [Tooltip("The generated plan for the problem.")]
        public PlanGeneration PlanGeneration;

        /// <summary>
        /// The active visualization instance.
        /// </summary>
        [System.NonSerialized]
        public Visualisation Visualisation;

        /// <summary>
        /// Fired whenever AnimationSpeed changes, with the new value.
        /// </summary>
        public event System.Action<float> OnAnimationSpeedChanged;

        /// <summary>
        /// The speed multiplier for animations.
        /// </summary>
        [Header("Settings")]
        [Range(0.1f, 5f)]
        [Tooltip("Multiplier for the animation speed.")]
        [SerializeField]
        private float _animationSpeed = 1.0f;

        public float AnimationSpeed
        {
            get => _animationSpeed;
            set
            {
                if (Mathf.Approximately(_animationSpeed, value)) return;
                _animationSpeed = value;
                OnAnimationSpeedChanged?.Invoke(_animationSpeed);
            }
        }

        /// <summary>
        /// Whether the visualization should automatically advance to the next step.
        /// Managed at runtime via SetContinuousMode().
        /// </summary>
        [System.NonSerialized]
        public bool AutoAdvance = false;

        /// <summary>
        /// Whether the visualization is currently paused.
        /// </summary>
        public bool IsPaused => Visualisation != null && Visualisation.VisState.IsPaused;

        // Event delegates and related lifecycle events for the visualization.

        /// <summary>
        /// Delegate for when the visualization is ready to start.
        /// </summary>
        public delegate void VisualisationReady(List<GroundedAction> actions);
        /// <summary>
        /// Event fired when the visualization is ready.
        /// </summary>
        public event VisualisationReady OnVisualisationReady;

        /// <summary>
        /// Delegate for when the initial state visualization begins.
        /// </summary>
        public delegate void VisualiseInitBlock();
        /// <summary>
        /// Event fired when the initial state visualization begins.
        /// </summary>
        public event VisualiseInitBlock OnVisualiseInitBlock;

        /// <summary>
        /// Delegate for when an initial state fluent visualization starts.
        /// </summary>
        public delegate void InitFluentStarted(string fluent, int index, int total);
        /// <summary>
        /// Event fired for every init fluent as it becomes the active step.
        /// </summary>
        public event InitFluentStarted OnInitFluentStarted;

        /// <summary>
        /// Delegate for when an action block visualization starts.
        /// </summary>
        public delegate void VisualisationActionBlock(string block, int i);
        /// <summary>
        /// Event fired for every plan action block as it becomes the active step.
        /// </summary>
        public event VisualisationActionBlock OnVisualisationActionBlock;

        /// <summary>
        /// Delegate for when all animations for a step have completed.
        /// </summary>
        public delegate void StepAnimationsComplete();
        /// <summary>
        /// Event fired once per completed step when not in continuous mode.
        /// </summary>
        public event StepAnimationsComplete OnStepAnimationsComplete;

        /// <summary>
        /// Delegate for when the entire visualization has finished.
        /// </summary>
        public delegate void VisualisationFinished();
        /// <summary>
        /// Event fired when the visualization is complete.
        /// </summary>
        public event VisualisationFinished OnVisualisationFinished;

        /// <summary>
        /// Delegate for when the timeline advances.
        /// </summary>
        public delegate void TimeLineAdvanced(double time, double progress);
        /// <summary>
        /// Event fired when the visualization timeline advances.
        /// </summary>
        public event TimeLineAdvanced OnTimeLineAdvanced;

        /// <summary>
        /// Starts the visualization process, beginning with the initial state.
        /// </summary>
        public void StartVisualisation()
        {
            _awaitingAdvance = false;
            _pendingAdvance = false;

            // Snapshot the initial state as a list so we can step through it.
            _initFluents = Visualisation.CurrentWorldState.State.Select(kvp => (kvp.Key, kvp.Value)).ToList();
            _initFluentIndex = -1;
            _initPhase = _initFluents.Count > 0;

            var actions = Visualisation.PlanResult.Plan?.Actions ?? new List<GroundedAction>();
            OnVisualisationReady?.Invoke(actions);
            OnVisualiseInitBlock?.Invoke();  // starts AnimationsLoop in AnimationsController

            // The AnimationsLoop will find an empty queue and fire OnTimePointAnimationEnd,
            // transitioning to the waiting state. The user then steps forward manually.
        }

        /// <summary>
        /// Advance one step. During the init phase this steps through the next
        /// init fluent; afterwards it advances the plan timeline.
        /// </summary>
        public void Advance()
        {
            if (_initPhase)
            {
                // Don't defer init steps — handle immediately so _animationsActive
                // is populated before the AnimationsLoop's next iteration.
                _awaitingAdvance = false;
                StepInitPhase();
            }
            else
            {
                _pendingAdvance = true;
            }
        }

        /// <summary>
        /// Enable or disable continuous (auto-advance) mode.
        /// If enabling while a step is already complete, schedules an advance.
        /// </summary>
        /// <param name="continuous">True for auto-advance, false for manual.</param>
        public void SetContinuousMode(bool continuous)
        {
            AutoAdvance = continuous;
            if (continuous && _awaitingAdvance)
                _pendingAdvance = true;
        }

        /// <summary>
        /// Pauses the current visualization.
        /// </summary>
        public void Pause() => Visualisation?.Pause();

        /// <summary>
        /// Resumes the current visualization.
        /// </summary>
        public void Resume() => Visualisation?.Resume();

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            AutoAdvance = false;    // always start in manual mode

            _animationsController = GetComponent<AnimationsController>();
        }

        private void Start()
        {
            if (Problem == null || PlanGeneration == null)
                return;

            Visualisation = new Visualisation(Problem.Proto, PlanGeneration.Proto);

            Visualisation.TimeLineAdvance += (_, args) =>
                OnTimeLineAdvanced?.Invoke(args.Time, args.Progress);

            // WorldStateChanged only fires for plan actions (not init fluents —
            // those are fed manually via UpdateQueue in StepInitPhase).
            Visualisation.WorldStateChanged += (_, args) =>
                _animationsController.UpdateQueue(args.AppliedAction, args.NewStateVar);

            Visualisation.ActionStarted += (_, action) =>
            {
                var actions = Visualisation.PlanResult.Plan?.Actions ?? new List<GroundedAction>();
                int index = actions.IndexOf(action);
                OnVisualisationActionBlock?.Invoke(action.ToString(), index);
            };

            Visualisation.VisualisationEnd += (_, __) =>
                OnVisualisationFinished?.Invoke();

            AnimationsController.Instance.OnTimePointAnimationEnd += () =>
            {
                if (_awaitingAdvance) return;
                _awaitingAdvance = true;

                if (AutoAdvance)
                    _pendingAdvance = true;
                else
                    OnStepAnimationsComplete?.Invoke();
            };
        }

        private void Update()
        {
            if (!_pendingAdvance || !_awaitingAdvance) return;

            _pendingAdvance = false;
            _awaitingAdvance = false;

            if (_initPhase)
                StepInitPhase();
            else
                Visualisation.Advance();
        }

        #endregion

        #region Private Internals

        private static Controller _instance;

        // Initialization phase step-by-step logic.
        //
        // Instead of queuing every init fluent at once, we treat each one as its
        // own step (identical to plan actions).  _initFluents holds the full list;
        // _initFluentIndex is the cursor; _initPhase is true until every fluent
        // has been stepped through.

        private List<(FluentExpression Fluent, object Value)> _initFluents;
        private int _initFluentIndex = -1;
        private bool _initPhase = false;

        // Logic for scheduling and debouncing simulation step advancements.
        //
        // OnTimePointAnimationEnd fires every frame while _animationsActive is
        // empty.  We never call visualisation.Advance() directly from inside an
        // event/coroutine callback because StartCoroutine runs synchronously to
        // its first yield, creating a same-frame chain that drains the whole plan.
        //
        // Instead: set _pendingAdvance = true → Update() processes it next frame.
        // _awaitingAdvance debounces the repeated OnTimePointAnimationEnd fires.

        private bool _pendingAdvance = false;
        private bool _awaitingAdvance = false;
        private AnimationsController _animationsController;

        private void QueueInitFluent(int index)
        {
            _initFluentIndex = index;
            var fluent = _initFluents[index];
            _animationsController.UpdateQueue(null, fluent);
            OnInitFluentStarted?.Invoke(fluent.ToString(), index, _initFluents.Count);
        }

        private void StepInitPhase()
        {
            int next = _initFluentIndex + 1;

            if (next < _initFluents.Count)
            {
                // More init fluents — queue the next one.
                // The AnimationsLoop is still running and will pick it up.
                QueueInitFluent(next);
            }
            else
            {
                // All init fluents have been stepped through.
                _initPhase = false;
                // _awaitingAdvance is already false (reset by caller).
                // The AnimationsLoop's next OnTimePointAnimationEnd fire will
                // transition to plan mode naturally: if autoAdvance → _pendingAdvance
                // → visualisation.Advance(); otherwise → OnStepAnimationsComplete.
            }
        }

        #endregion
    }
}
