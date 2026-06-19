using GeTPlan.Core.Models;
using GeTPlan.Core.Models.Expressions;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using PDSim.Utils;

namespace PDSim.Components
{
    /// <summary>
    /// Controls the execution of animations in the simulation.
    /// Manages the animation queue and triggers animations based on state changes.
    ///
    /// Step-completion model (event-driven, no polling):
    ///   1. Controller.Update() calls Visualisation.Advance() → WorldStateChanged fires
    ///      synchronously for each effect → UpdateQueue populates _animationsActive.
    ///   2. Controller then calls BeginStep() which starts one AnimationMachineLoop
    ///      coroutine per active context.  If there are no active routines it schedules
    ///      a one-frame SignalStepComplete coroutine.
    ///   3. Each machine loop drives its AnimationRoutine to completion, then removes
    ///      itself from _animationsActive.  When the last routine finishes it fires
    ///      OnTimePointAnimationEnd exactly once.
    ///   4. Controller's OnTimePointAnimationEnd handler sets _awaitingAdvance so the
    ///      next Update() can advance the plan.
    /// </summary>
    public class AnimationsController : MonoBehaviour
    {
        #region Public API

        /// <summary>
        /// Singleton instance of the AnimationsController.
        /// </summary>
        public static AnimationsController Instance
        {
            get
            {
                if (_instance == null)
                    _instance = FindAnyObjectByType<AnimationsController>();
                return _instance;
            }
        }

        /// <summary>
        /// Represents the state of an animation routine.
        /// </summary>
        public enum AnimationState
        {
            None,
            Ready,
            Running,
            End,
            Finished
        }

        /// <summary>
        /// Delegate for visualization step events.
        /// </summary>
        /// <param name="predicate">The predicate being visualized.</param>
        public delegate void VisualisationStep(string predicate);

        /// <summary>
        /// Event fired when a visualization step begins.
        /// </summary>
        public event VisualisationStep OnVisualisationStep;

        /// <summary>
        /// Delegate for animation end events.
        /// </summary>
        public delegate void AnimationEnd();

        /// <summary>
        /// Event fired once when all animations for the current time point have finished.
        /// </summary>
        public event AnimationEnd OnTimePointAnimationEnd;

        /// <summary>
        /// Updates the animation queue with new state variables resulting from an action.
        /// If the action has a matching action animation, that runs instead of predicate animations.
        /// During init phase (action == null) predicate animations always run.
        ///
        /// Call BeginStep() after all UpdateQueue calls for a step are complete.
        /// </summary>
        /// <param name="action">The grounded action that caused the state change, or null during init.</param>
        /// <param name="newStateVar">The new fluent state variable and its value.</param>
        public void UpdateQueue(GroundedAction action, (FluentExpression Fluent, object Value) newStateVar)
        {
            // Init phase: always use predicate animations
            if (action == null)
            {
                EnqueueFluentAnimation(null, newStateVar);
                return;
            }

            // Plan phase: check whether this action has a dedicated animation
            if (_actionAnimations != null)
            {
                var actionMatch = _actionAnimations.AnimationCheck(action);
                if (actionMatch != null && actionMatch.Count > 0)
                {
                    // Queue the action animation once — deduplicate across multiple effect callbacks.
                    // Use Id when available; fall back to ToString() since planners often leave Id empty.
                    var actionKey = GetContextKey(action);
                    if (!_queuedActionIds.Contains(actionKey))
                    {
                        _queuedActionIds.Add(actionKey);
                        EnqueueActionAnimation(action, actionMatch);
                    }
                    // Suppress predicate animations for every effect of this action
                    return;
                }
            }

            // Fallthrough: no action animation defined — use predicate animations
            EnqueueFluentAnimation(action, newStateVar);
        }

        /// <summary>
        /// Starts an AnimationMachineLoop coroutine for every queued animation context.
        /// Call this after all UpdateQueue calls for a step are complete.
        ///
        /// If no animations are queued for this step, schedules a one-frame completion
        /// signal so the controller can advance without stalling.
        /// </summary>
        public void BeginStep()
        {
            if (_animationsActive.Count == 0)
            {
                // Nothing to animate — signal completion on the next frame.
                StartCoroutine(SignalStepComplete());
                return;
            }

            // Snapshot keys because AnimationMachineLoop may remove its key before
            // iteration completes in degenerate cases (empty queue at start).
            var contexts = new List<string>(_animationsActive.Keys);
            foreach (var context in contexts)
            {
                if (_animationsActive.TryGetValue(context, out var routine)
                    && routine.State == AnimationState.None)
                {
                    StartCoroutine(AnimationMachineLoop(context));
                }
            }
        }

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            _animations = PredicateAnimations.Instance;
            _actionAnimations = ActionAnimations.Instance;
            _objects = ProblemObjects.Instance;
            _animationsActive = new Dictionary<string, AnimationRoutine>();
        }

        private void Start()
        {
            Controller.Instance.OnVisualiseInitBlock += () =>
            {
                _queuedActionIds.Clear();
                _stop = false;
                // No loop to restart — BeginStep() is called explicitly per step.
            };

            Controller.Instance.OnVisualisationFinished += () =>
            {
                _stop = true;
            };
        }

        #endregion

        #region Private Internals

        private static AnimationsController _instance;

        private ProblemObjects _objects;
        private PredicateAnimations _animations;
        private ActionAnimations _actionAnimations;
        private Dictionary<string, AnimationRoutine> _animationsActive;
        private Dictionary<string, GameObject> _activeGraphs = new Dictionary<string, GameObject>();
        private readonly HashSet<string> _queuedActionIds = new HashSet<string>();
        private bool _stop = false;

        // Cache for TryLoadComponent reflection results — resolved once per scriptClassName.
        private static readonly Dictionary<string, System.Type> _resolvedTypes
            = new Dictionary<string, System.Type>();

        /// <summary>
        /// Returns a stable context key for an action that is consistent across
        /// UpdateQueue dedup, EnqueueX, and _animationsActive keying.
        /// Falls back to action.ToString() when Id is empty (planners often omit it).
        /// </summary>
        private static string GetContextKey(GroundedAction action)
            => !string.IsNullOrEmpty(action.Id) ? action.Id : action.ToString();

        /// <summary>
        /// Fires OnTimePointAnimationEnd on the next frame when there are no active
        /// animations for this step.
        /// </summary>
        private IEnumerator SignalStepComplete()
        {
            yield return null;
            if (!_stop)
                OnTimePointAnimationEnd?.Invoke();
        }

        /// <summary>
        /// Drives a single animation context through its queue sequentially.
        /// Removes itself from _animationsActive on completion and fires
        /// OnTimePointAnimationEnd when the last context finishes.
        /// </summary>
        private IEnumerator AnimationMachineLoop(string context)
        {
            if (!_animationsActive.TryGetValue(context, out var routine))
                yield break;

            routine.State = AnimationState.None;

            while (routine.State != AnimationState.Finished)
            {
                switch (routine.State)
                {
                    case AnimationState.None:
                        routine.State = routine.Queue.Count > 0
                            ? AnimationState.Ready
                            : AnimationState.Finished;
                        break;

                    case AnimationState.Ready:
                        var animation = routine.Queue.Dequeue();
                        OnVisualisationStep?.Invoke(animation.FluentString);
                        TriggerAnimation(animation, context, animation.ParametersObjects);
                        // Always yield after triggering so that synchronous AnimationEndHandler
                        // calls (no-visualizer fallback) don't skip a frame of state processing.
                        yield return null;
                        break;

                    case AnimationState.Running:
                        yield return null;
                        break;

                    case AnimationState.End:
                        routine.State = routine.Queue.Count == 0
                            ? AnimationState.None
                            : AnimationState.Ready;
                        yield return null;
                        break;

                    default:
                        yield return null;
                        break;
                }
            }

            // This context is done — remove it and check if all contexts are complete.
            _animationsActive.Remove(context);
            if (_animationsActive.Count == 0 && !_stop)
                OnTimePointAnimationEnd?.Invoke();
        }

        private void AnimationEndHandler(string context)
        {
            if (!_animationsActive.ContainsKey(context)) return;
            _animationsActive[context].State = AnimationState.End;

            if (_activeGraphs.TryGetValue(context, out var graph))
            {
                SimpleObjectPool.Instance.Return(graph);
                _activeGraphs.Remove(context);
            }
        }

        private void EnqueueFluentAnimation(GroundedAction action, (FluentExpression Fluent, object Value) newStateVar)
        {
            var match = _animations.AnimationCheck(newStateVar);
            if (match == null || match.Count == 0)
                return;

            string context = action != null ? GetContextKey(action) : "init";

            foreach (var animationData in match)
            {
                if (!_animationsActive.ContainsKey(context))
                    _animationsActive.Add(context, new AnimationRoutine());

                float duration = 0f;
                if (action != null && action.StartTime.HasValue && action.EndTime.HasValue)
                    duration = (float)(action.EndTime.Value - action.StartTime.Value);

                var parameters = newStateVar.Fluent.Arguments
                    .Select(a => a is ConstantExpression c ? c.Value.ToString() : a.ToString())
                    .Select(p => _objects.GetObjectInScene(p))
                    .Where(v => v != null)
                    .ToArray();

                _animationsActive[context].Queue.Enqueue(new AnimationQueueElement()
                {
                    AnimationName = animationData.Name,
                    FluentString = newStateVar.Fluent.ToString(),
                    Value = newStateVar.Value,
                    ParametersObjects = parameters,
                    GraphToClone = animationData.SceneObjectReference,
                    ScriptClassName = animationData.ScriptClassName,
                    Duration = duration,
                    IsActionAnimation = false
                });
            }
        }

        private void EnqueueActionAnimation(GroundedAction action, List<ActionAnimation.AnimationData> match)
        {
            string context = GetContextKey(action);
            if (!_animationsActive.ContainsKey(context))
                _animationsActive.Add(context, new AnimationRoutine());

            float duration = 0f;
            if (action.StartTime.HasValue && action.EndTime.HasValue)
                duration = (float)(action.EndTime.Value - action.StartTime.Value);

            var parameters = action.Objects
                .Select(o => _objects.GetObjectInScene(o.Name))
                .Where(v => v != null)
                .ToArray();

            foreach (var animData in match)
            {
                _animationsActive[context].Queue.Enqueue(new AnimationQueueElement()
                {
                    AnimationName = animData.Name,
                    FluentString = action.ToString(),
                    Value = null,
                    ParametersObjects = parameters,
                    GraphToClone = animData.SceneObjectReference,
                    ScriptClassName = animData.ScriptClassName,
                    Duration = duration,
                    IsActionAnimation = true
                });
            }
        }

        /// <summary>
        /// Resolves a component type by scriptClassName, caching the result so the
        /// expensive assembly scan only runs once per class name.
        /// </summary>
        private T TryLoadComponent<T>(GameObject target, string scriptClassName) where T : class
        {
            if (!_resolvedTypes.TryGetValue(scriptClassName, out var type))
            {
                foreach (var assembly in System.AppDomain.CurrentDomain.GetAssemblies())
                {
                    type = assembly.GetType(scriptClassName)
                        ?? assembly.GetType("GeneratedVisualizers." + scriptClassName);
                    if (type != null) break;
                }
                // Cache even on miss (null) to avoid re-scanning for unknown names.
                _resolvedTypes[scriptClassName] = type;
            }

            if (type != null)
                return target.AddComponent(type) as T;
            return null;
        }

        private void TriggerAnimation(AnimationQueueElement animationElement, string context, VisualisationObject[] objects)
        {
            var animationInstance = SimpleObjectPool.Instance.Get(animationElement.GraphToClone);

#if UNITY_EDITOR
            // Renaming GameObjects has a managed-string cost; keep it editor-only for debugging.
            animationInstance.name = $"{context} === {animationElement.AnimationName}";
#endif

            // Guard against duplicate-key: return any pre-existing graph for this context
            // to the pool before tracking the new one.
            if (_activeGraphs.TryGetValue(context, out var staleGraph))
            {
                Debug.LogWarning($"[PDSim] TriggerAnimation: replacing stale graph for context '{context}' — returning to pool.");
                SimpleObjectPool.Instance.Return(staleGraph);
            }
            _activeGraphs[context] = animationInstance;

            if (animationElement.IsActionAnimation)
            {
                var actionVisualizer = animationInstance.GetComponent<IActionVisualizer>();
                if (actionVisualizer == null && !string.IsNullOrEmpty(animationElement.ScriptClassName))
                    actionVisualizer = TryLoadComponent<IActionVisualizer>(animationInstance, animationElement.ScriptClassName);

                if (actionVisualizer != null)
                {
                    _animationsActive[context].State = AnimationState.Running;
                    actionVisualizer.Animate(
                        new List<string>(animationElement.ParametersObjects
                            .Select(o => o != null ? o.name : string.Empty)),
                        objects,
                        animationElement.Duration,
                        () => AnimationEndHandler(context)
                    );
                    return;
                }
            }
            else
            {
                var fluentVisualizer = animationInstance.GetComponent<IFluentVisualizer>();
                if (fluentVisualizer == null && !string.IsNullOrEmpty(animationElement.ScriptClassName))
                    fluentVisualizer = TryLoadComponent<IFluentVisualizer>(animationInstance, animationElement.ScriptClassName);

                if (fluentVisualizer != null)
                {
                    _animationsActive[context].State = AnimationState.Running;
                    fluentVisualizer.Animate(
                        new List<string>(),
                        animationElement.Value,
                        objects,
                        animationElement.Duration,
                        () => AnimationEndHandler(context)
                    );
                    return;
                }
            }

            // No visualizer found — treat as instant completion.
            AnimationEndHandler(context);
        }

        private class AnimationRoutine
        {
            public AnimationState State { get; set; }
            public Queue<AnimationQueueElement> Queue { get; set; }

            public AnimationRoutine()
            {
                State = AnimationState.None;
                Queue = new Queue<AnimationQueueElement>();
            }
        }

        private class AnimationQueueElement
        {
            public string AnimationName { get; set; }
            public string FluentString { get; set; }
            public object Value { get; set; }
            public VisualisationObject[] ParametersObjects { get; set; }
            public GameObject GraphToClone { get; set; }
            public string ScriptClassName { get; set; }
            public float Duration { get; set; }
            public bool IsActionAnimation { get; set; }
        }

        #endregion
    }
}
