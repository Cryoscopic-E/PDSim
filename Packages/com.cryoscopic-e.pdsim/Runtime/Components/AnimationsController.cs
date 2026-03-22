using GeTPlan.Core.Models;
using GeTPlan.Core.Logic;
using GeTPlan.Core.Models.Expressions;
using PDSimAPI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using PDSim.Utils;

namespace PDSim.Components
{
    public class AnimationsController : MonoBehaviour
    {
        private const float ANIMATION_INTERVAL = 0.1f;

        // Singleton Instance
        // ------------------

        private static AnimationsController _instance;
        public static AnimationsController Instance
        {
            get
            {
                if (_instance == null)
                    _instance = FindAnyObjectByType<AnimationsController>();
                return _instance;
            }
        }

        public enum AnimationState
        {
            None,
            Ready,
            Running,
            End,
            Finished
        }

        private ProblemObjects _objects;

        private Animations _animations;

        private Dictionary<string, AnimationRoutine> _animationsActive;

        public delegate void VisualisationStep(string predicate);
        public event VisualisationStep OnVisualisationStep;

        public delegate void AnimationEnd();
        public event AnimationEnd OnTimePointAnimationEnd;

        private bool stop = false;

        private Coroutine _loopCoroutine;

        private void Awake()
        {
            _animations = Animations.Instance;
            _objects = ProblemObjects.Instance;
            _animationsActive = new Dictionary<string, AnimationRoutine>();
        }

        private void RestartLoop()
        {
            if (_loopCoroutine != null)
                StopCoroutine(_loopCoroutine);
            _loopCoroutine = StartCoroutine(AnimationsLoop());
        }

        private void Start()
        {
            Controller.Instance.OnVisualiseInitBlock += () =>
            {
                stop = false;
                RestartLoop();
            };

            Controller.Instance.OnVisualisationActionBlock += (block, i) =>
            {
                RestartLoop();
            };

            Controller.Instance.OnVisualisationFinished += () =>
            {
                stop = true;
            };
        }

        private IEnumerator AnimationsLoop()
        {
            yield return null;

            while (!stop)
            {
                if (_animationsActive.Count == 0)
                {
                    OnTimePointAnimationEnd?.Invoke();
                    yield return null;
                }

                var toRemove = new List<string>();
                foreach (var animationRoutine in _animationsActive)
                {
                    if (animationRoutine.Value.state == AnimationState.Finished)
                    {
                        toRemove.Add(animationRoutine.Key);
                    }
                    else if (animationRoutine.Value.state == AnimationState.None)
                    {
                        StartCoroutine(AnimationMachineLoop(animationRoutine.Key));
                    }
                }
                foreach (var key in toRemove)
                {
                    _animationsActive.Remove(key);
                }

                yield return new WaitForSeconds(ANIMATION_INTERVAL);
            }
        }


        public void UpdateQueue(GroundedAction action, (FluentExpression Fluent, object Value) newStateVar)
        {
            var match = _animations.AnimationCheck(newStateVar);

            if (match == null || match.Count == 0)
            {
                return;
            }

            string context = action != null ? action.Id : "init";

            foreach (var animationData in match)
            {
                if (!_animationsActive.ContainsKey(context))
                {
                    _animationsActive.Add(context, new AnimationRoutine());
                }

                float duration = 0f;
                if (action != null && action.StartTime.HasValue && action.EndTime.HasValue)
                {
                    duration = (float)(action.EndTime.Value - action.StartTime.Value);
                }

                var parameters = newStateVar.Fluent.Arguments
                    .Select(a => a is ConstantExpression c ? c.Value.ToString() : a.ToString())
                    .Select(p => _objects.GetObjectInScene(p)?.gameObject)
                    .Where(g => g != null)
                    .ToArray();

                _animationsActive[context].queue.Enqueue(new AnimationQueueElement()
                {
                    animationName = animationData.name,
                    fluentString = newStateVar.Fluent.ToString(),
                    value = newStateVar.Value,
                    parametersObjects = parameters,
                    graphToClone = animationData.sceneObjectReference,
                    scriptClassName = animationData.scriptClassName,
                    duration = duration
                });
            }
        }

        private IEnumerator AnimationMachineLoop(string context)
        {
            if (!_animationsActive.ContainsKey(context))
                yield break;
            _animationsActive[context].state = AnimationState.None;
            while (_animationsActive[context].state != AnimationState.Finished)
            {
                switch (_animationsActive[context].state)
                {
                    case AnimationState.None:
                        if (_animationsActive[context].queue.Count > 0)
                            _animationsActive[context].state = AnimationState.Ready;
                        else
                            _animationsActive[context].state = AnimationState.Finished;
                        break;
                    case AnimationState.Ready:
                        var animation = _animationsActive[context].queue.Dequeue();
                        OnVisualisationStep?.Invoke(animation.fluentString); 
                        TriggerAnimation(animation, context, animation.parametersObjects);
                        break;
                    case AnimationState.Running:
                        yield return null;
                        break;
                    case AnimationState.End:
                        if (_animationsActive[context].queue.Count == 0)
                            _animationsActive[context].state = AnimationState.None;
                        else
                            _animationsActive[context].state = AnimationState.Ready;
                        break;
                    case AnimationState.Finished:
                    default:
                        yield return null;
                        break;
                }
                yield return new WaitForEndOfFrame();
            }
        }
        
        Dictionary<string, GameObject> _activeGraphs = new Dictionary<string, GameObject>();
        
        private void AnimationEndHandler(string context)
        {
            if (!_animationsActive.ContainsKey(context)) return;
            _animationsActive[context].state = AnimationState.End;

            if (_activeGraphs.ContainsKey(context))
            {
                var graph = _activeGraphs[context];
                SimpleObjectPool.Instance.Return(graph);
                _activeGraphs.Remove(context);
            }
        }

        private void TriggerAnimation(AnimationQueueElement animationElement, string context, GameObject[] objects)
        {
            var animationInstance = SimpleObjectPool.Instance.Get(animationElement.graphToClone);

            animationInstance.name = $"{context} === {animationElement.animationName}";
            _activeGraphs.Add(context, animationInstance);

            var scriptVisualizer = animationInstance.GetComponent<IFluentVisualizer>();

            if (scriptVisualizer == null && !string.IsNullOrEmpty(animationElement.scriptClassName))
            {
                System.Type type = null;
                foreach (var assembly in System.AppDomain.CurrentDomain.GetAssemblies())
                {
                    type = assembly.GetType(animationElement.scriptClassName)
                        ?? assembly.GetType("GeneratedVisualizers." + animationElement.scriptClassName);
                    if (type != null) break;
                }
                if (type != null)
                {
                    scriptVisualizer = animationInstance.AddComponent(type) as IFluentVisualizer;
                }
            }

            if (scriptVisualizer != null)
            {
                scriptVisualizer.Animate(
                    new List<string>(),
                    animationElement.value,
                    objects,
                    animationElement.duration,
                    () => AnimationEndHandler(context)
                );
                _animationsActive[context].state = AnimationState.Running;
                return;
            }

            AnimationEndHandler(context);
            _animationsActive[context].state = AnimationState.Running;
        }

        private class AnimationRoutine
        {
            public AnimationState state;
            public Queue<AnimationQueueElement> queue;

            public AnimationRoutine()
            {
                state = AnimationState.None;
                queue = new Queue<AnimationQueueElement>();
            }
        }

        private class AnimationQueueElement
        {
            public string animationName;
            public string fluentString;
            public object value;
            public GameObject[] parametersObjects;
            public GameObject graphToClone;
            public string scriptClassName;
            public float duration;
        }
    }
}
