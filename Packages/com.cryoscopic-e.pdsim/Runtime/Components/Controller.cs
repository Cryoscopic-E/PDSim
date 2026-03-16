using GeTModel;
using PDSim.ScriptableObjects;
using PDSimAPI;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace PDSim.Components
{
    public class Controller : MonoBehaviour
    {
        // Singleton Instance
        // ------------------

        private static Controller _instance;
        public static Controller Instance
        {
            get
            {
                if (_instance == null)
                    _instance = FindAnyObjectByType<Controller>();
                return _instance;
            }
        }

        // Data Assets
        // -----------

        public PlanningProblem problem;
        public PlanGeneration planGeneration;
        [System.NonSerialized]
        public Visualisation visualisation;

        [Header("Settings")]
        [Range(0.1f, 5f)]
        public float animationSpeed = 1.0f;

        // Managed at runtime via SetContinuousMode(); [NonSerialized] so an old
        // scene value of true can never bypass the manual-step default.
        [System.NonSerialized]
        public bool autoAdvance = false;

        public bool IsPaused => visualisation != null && visualisation.VisState.IsPaused;

        // ── Init-phase step-through ──────────────────────────────────────────────
        //
        // Instead of queuing every init fluent at once, we treat each one as its
        // own step (identical to plan actions).  _initFluents holds the full list;
        // _initFluentIndex is the cursor; _initPhase is true until every fluent
        // has been stepped through.

        private List<GeTStateVariable> _initFluents;
        private int  _initFluentIndex = -1;
        private bool _initPhase       = false;

        // ── Advance scheduling ───────────────────────────────────────────────────
        //
        // OnTimePointAnimationEnd fires every frame while _animationsActive is
        // empty.  We never call visualisation.Advance() directly from inside an
        // event/coroutine callback because StartCoroutine runs synchronously to
        // its first yield, creating a same-frame chain that drains the whole plan.
        //
        // Instead: set _pendingAdvance = true → Update() processes it next frame.
        // _awaitingAdvance debounces the repeated OnTimePointAnimationEnd fires.

        private bool _pendingAdvance  = false;
        private bool _awaitingAdvance = false;

        private ProblemObjects      _problemObjects;
        private InitBlock           _initBlock;
        private TypeHierarchy       _typeHierarchy;
        private AnimationsController _animationsController;

        // Events and Delegates
        // --------------------

        public delegate void VisualisationReady(List<GeTActionInstance> actions);
        public event VisualisationReady OnVisualisationReady;

        public delegate void VisualiseInitBlock();
        public event VisualiseInitBlock OnVisualiseInitBlock;

        // Fired for every init fluent as it becomes the active step.
        public delegate void InitFluentStarted(string fluent, int index, int total);
        public event InitFluentStarted OnInitFluentStarted;

        public delegate void VisualisationtionActionBlock(string block, int i);
        public event VisualisationtionActionBlock OnVisualisationActionBlock;

        // Fired once per completed step when not in continuous mode.
        public delegate void StepAnimationsComplete();
        public event StepAnimationsComplete OnStepAnimationsComplete;

        public delegate void VisualisationFinished();
        public event VisualisationFinished OnVisualisationFinished;

        public delegate void TimeLineAdvanced(double time, double progress);
        public event TimeLineAdvanced OnTimeLineAdvanced;

        // ── Lifecycle ────────────────────────────────────────────────────────────

        private void Awake()
        {
            autoAdvance = false;    // always start in manual mode

            _problemObjects       = ProblemObjects.Instance;
            _initBlock            = InitBlock.Instance;
            _typeHierarchy        = TypeHierarchy.Instance;
            _animationsController = GetComponent<AnimationsController>();
        }

        private void Start()
        {
            if (problem == null || planGeneration == null)
                return;

            visualisation = new Visualisation(problem.proto, planGeneration.proto);

            visualisation.TimeLineAdvance += (_, args) =>
                OnTimeLineAdvanced?.Invoke(args.Time.ToDouble(), args.Progress);

            // WorldStateChanged only fires for plan actions (not init fluents —
            // those are fed manually via UpdateQueue in StepInitPhase).
            visualisation.WorldStateChanged += (_, args) =>
                _animationsController.UpdateQueue(args.AppliedAction, args.NewStateVar);

            visualisation.ActionStarted += (_, action) =>
            {
                int index = visualisation.PlanGeneration.Plan.Actions.IndexOf(action);
                OnVisualisationActionBlock?.Invoke(action.ToString(), index);
            };

            visualisation.VisualisationEnd += (_, __) =>
                OnVisualisationFinished?.Invoke();

            AnimationsController.Instance.OnTimePointAnimationEnd += () =>
            {
                if (_awaitingAdvance) return;
                _awaitingAdvance = true;

                if (autoAdvance)
                    _pendingAdvance = true;
                else
                    OnStepAnimationsComplete?.Invoke();
            };
        }

        // Update processes deferred advances — one per frame maximum.
        private void Update()
        {
            if (!_pendingAdvance || !_awaitingAdvance) return;

            _pendingAdvance  = false;
            _awaitingAdvance = false;

            if (_initPhase)
                StepInitPhase();
            else
                visualisation.Advance();
        }

        // ── Public API ───────────────────────────────────────────────────────────

        public void StartVisualisation()
        {
            _awaitingAdvance  = false;
            _pendingAdvance   = false;

            // Snapshot the initial state as a list so we can step through it.
            _initFluents      = visualisation.CurrentWorldState.State.ToList();
            _initFluentIndex  = -1;
            _initPhase        = _initFluents.Count > 0;

            OnVisualisationReady?.Invoke(visualisation.PlanGeneration.Plan.Actions);
            OnVisualiseInitBlock?.Invoke();  // starts AnimationsLoop in AnimationsController

            // Queue the first init fluent so the loop has something to process.
            if (_initPhase)
                QueueInitFluent(0);
            // If there are no init fluents the loop will fire OnTimePointAnimationEnd
            // naturally and transition straight to the plan.
        }

        /// <summary>
        /// Advance one step.  During the init phase this steps through the next
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
        public void SetContinuousMode(bool continuous)
        {
            autoAdvance = continuous;
            if (continuous && _awaitingAdvance)
                _pendingAdvance = true;
        }

        public void Pause()  => visualisation?.Pause();
        public void Resume() => visualisation?.Resume();

        // ── Init-phase internals ─────────────────────────────────────────────────

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
    }
}
