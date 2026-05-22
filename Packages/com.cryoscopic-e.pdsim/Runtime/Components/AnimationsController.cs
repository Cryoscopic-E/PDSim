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
        /// Event fired when all animations for a time point have finished.
        /// </summary>
        public event AnimationEnd OnTimePointAnimationEnd;

        /// <summary>
        /// Updates the animation queue with new state variables resulting from an action.
        /// If the action has a matching action animation, that runs instead of predicate animations.
        /// During init phase (action == null) predicate animations always run.
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
                    // Queue the action animation once — deduplicate across multiple effect callbacks
                    if (!_queuedActionIds.Contains(action.Id))
                    {
                        _queuedActionIds.Add(action.Id);
                        EnqueueActionAnimation(action, actionMatch);
                    }
                    // Suppress predicate animations for every effect of this action
                    return;
                }
            }

            // Fallthrough: no action animation defined — use predicate animations
            EnqueueFluentAnimation(action, newStateVar);
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
                RestartLoop();
            };

            Controller.Instance.OnVisualisationActionBlock += (block, i) =>
            {
                RestartLoop();
            };

            Controller.Instance.OnVisualisationFinished += () =>
            {
                _stop = true;
            };
        }

        #endregion

        #region Private Internals

        private const float AnimationInterval = 0.1f;

        private static AnimationsController _instance;

        private ProblemObjects _objects;
        private PredicateAnimations _animations;
        private ActionAnimations _actionAnimations;
        private Dictionary<string, AnimationRoutine> _animationsActive;
        private Dictionary<string, GameObject> _activeGraphs = new Dictionary<string, GameObject>();
        private readonly HashSet<string> _queuedActionIds = new HashSet<string>();
        private bool _stop = false;
        private Coroutine _loopCoroutine;

        private void RestartLoop()
        {
            if (_loopCoroutine != null)
                StopCoroutine(_loopCoroutine);
            _loopCoroutine = StartCoroutine(AnimationsLoop());
        }

        private IEnumerator AnimationsLoop()
        {
            yield return null;

            while (!_stop)
            {
                if (_animationsActive.Count == 0)
                {
                    OnTimePointAnimationEnd?.Invoke();
                    yield return null;
                }

                var toRemove = new List<string>();
                foreach (var animationRoutine in _animationsActive)
                {
                    if (animationRoutine.Value.State == AnimationState.Finished)
                    {
                        toRemove.Add(animationRoutine.Key);
                    }
                    else if (animationRoutine.Value.State == AnimationState.None)
                    {
                        StartCoroutine(AnimationMachineLoop(animationRoutine.Key));
                    }
                }
                foreach (var key in toRemove)
                {
                    _animationsActive.Remove(key);
                }

                yield return new WaitForSeconds(AnimationInterval);
            }
        }

        private IEnumerator AnimationMachineLoop(string context)
        {
            if (!_animationsActive.ContainsKey(context))
                yield break;
            _animationsActive[context].State = AnimationState.None;
            while (_animationsActive[context].State != AnimationState.Finished)
            {
                switch (_animationsActive[context].State)
                {
                    case AnimationState.None:
                        if (_animationsActive[context].Queue.Count > 0)
                            _animationsActive[context].State = AnimationState.Ready;
                        else
                            _animationsActive[context].State = AnimationState.Finished;
                        break;
                    case AnimationState.Ready:
                        var animation = _animationsActive[context].Queue.Dequeue();
                        OnVisualisationStep?.Invoke(animation.FluentString);
                        TriggerAnimation(animation, context, animation.ParametersObjects);
                        break;
                    case AnimationState.Running:
                        yield return null;
                        break;
                    case AnimationState.End:
                        if (_animationsActive[context].Queue.Count == 0)
                            _animationsActive[context].State = AnimationState.None;
                        else
                            _animationsActive[context].State = AnimationState.Ready;
                        break;
                    case AnimationState.Finished:
                    default:
                        yield return null;
                        break;
                }
                yield return new WaitForEndOfFrame();
            }
        }

        private void AnimationEndHandler(string context)
        {
            if (!_animationsActive.ContainsKey(context)) return;
            _animationsActive[context].State = AnimationState.End;

            if (_activeGraphs.ContainsKey(context))
            {
                var graph = _activeGraphs[context];
                SimpleObjectPool.Instance.Return(graph);
                _activeGraphs.Remove(context);
            }
        }

        private void EnqueueFluentAnimation(GroundedAction action, (FluentExpression Fluent, object Value) newStateVar)
        {
            var match = _animations.AnimationCheck(newStateVar);
            if (match == null || match.Count == 0)
                return;

            string context = action != null ? action.Id : "init";

            foreach (var animationData in match)
            {
                if (!_animationsActive.ContainsKey(context))
                    _animationsActive.Add(context, new AnimationRoutine());

                float duration = 0f;
                if (action != null && action.StartTime.HasValue && action.EndTime.HasValue)
                    duration = (float)(action.EndTime.Value - action.StartTime.Value);

                var parameters = newStateVar.Fluent.Arguments
                    .Select(a => a is ConstantExpression c ? c.Value.ToString() : a.ToString())
                    .Select(p => _objects.GetObjectInScene(p)?.gameObject)
                    .Where(g => g != null)
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
            string context = action.Id;
            if (!_animationsActive.ContainsKey(context))
                _animationsActive.Add(context, new AnimationRoutine());

            float duration = 0f;
            if (action.StartTime.HasValue && action.EndTime.HasValue)
                duration = (float)(action.EndTime.Value - action.StartTime.Value);

            var parameters = action.Objects
                .Select(o => _objects.GetObjectInScene(o.Name)?.gameObject)
                .Where(g => g != null)
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

        private T TryLoadComponent<T>(GameObject target, string scriptClassName) where T : class
        {
            System.Type type = null;
            foreach (var assembly in System.AppDomain.CurrentDomain.GetAssemblies())
            {
                type = assembly.GetType(scriptClassName)
                    ?? assembly.GetType("GeneratedVisualizers." + scriptClassName);
                if (type != null) break;
            }
            if (type != null)
                return target.AddComponent(type) as T;
            return null;
        }

        private void TriggerAnimation(AnimationQueueElement animationElement, string context, GameObject[] objects)
        {
            var animationInstance = SimpleObjectPool.Instance.Get(animationElement.GraphToClone);
            animationInstance.name = $"{context} === {animationElement.AnimationName}";
            _activeGraphs.Add(context, animationInstance);

            if (animationElement.IsActionAnimation)
            {
                var actionVisualizer = animationInstance.GetComponent<IActionVisualizer>();
                if (actionVisualizer == null && !string.IsNullOrEmpty(animationElement.ScriptClassName))
                    actionVisualizer = TryLoadComponent<IActionVisualizer>(animationInstance, animationElement.ScriptClassName);

                if (actionVisualizer != null)
                {
                    actionVisualizer.Animate(
                        new List<string>(animationElement.ParametersObjects
                            .Select(o => o != null ? o.name : string.Empty)),
                        objects,
                        animationElement.Duration,
                        () => AnimationEndHandler(context)
                    );
                    _animationsActive[context].State = AnimationState.Running;
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
                    fluentVisualizer.Animate(
                        new List<string>(),
                        animationElement.Value,
                        objects,
                        animationElement.Duration,
                        () => AnimationEndHandler(context)
                    );
                    _animationsActive[context].State = AnimationState.Running;
                    return;
                }
            }

            AnimationEndHandler(context);
            _animationsActive[context].State = AnimationState.Running;
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
            public GameObject[] ParametersObjects { get; set; }
            public GameObject GraphToClone { get; set; }
            public string ScriptClassName { get; set; }
            public float Duration { get; set; }
            public bool IsActionAnimation { get; set; }
        }

        #endregion
    }
}
