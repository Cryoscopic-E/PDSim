using GeTModel;
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

        //private Queue<AnimationQueueElement> animationQueue  = new Queue<AnimationQueueElement>();

        // OnVisualisationStep is called when the visualisation is animating a single fluent
        public delegate void VisualisationStep(string predicate);
        public event VisualisationStep OnVisualisationStep;

        // OnVisualisationEnd is called when the all the queues in animationsActive are empty
        public delegate void AnimationEnd();
        public event AnimationEnd OnTimePointAnimationEnd;

        private bool stop = false;

        private void Awake()
        {
            _animations = Animations.Instance;
            _objects = ProblemObjects.Instance;
            _animationsActive = new Dictionary<string, AnimationRoutine>();
        }

        private void Start()
        {
            Controller.Instance.OnVisualiseInitBlock += () =>
            {
                stop = false;
                StartCoroutine(AnimationsLoop());
            };

            Controller.Instance.OnVisualisationActionBlock += (block, i) =>
            {
                StartCoroutine(AnimationsLoop());
            };

            Controller.Instance.OnVisualisationFinished += () =>
            {
                stop = true;
            };
            // Constantly check if the animationActive dictionary contains any queues

        }

        private IEnumerator AnimationsLoop()
        {
            while (!stop)
            {
                // Check if the dictionary is empty
                if (_animationsActive.Count == 0)
                {
                    OnTimePointAnimationEnd?.Invoke(); // Added safe invoke
                    yield return null;
                }

                // Check is the animation routine state is finished
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


        public void UpdateQueue(GeTActionInstance action, GeTStateVariable newStateVar)
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
                if (action != null && action.StartTime != null && action.EndTime != null)
                {
                    duration = (float)(action.EndTime.ToDouble() - action.StartTime.ToDouble());
                }

                _animationsActive[context].queue.Enqueue(new AnimationQueueElement()
                {
                    animationName = animationData.name,
                    fluentString = newStateVar.Fluent.ToString(),
                    value = newStateVar.Value.Atom,
                    parametersObjects = newStateVar.GetParameters().Select(p => _objects.GetObjectInScene(p).gameObject).ToArray(),
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
                        OnVisualisationStep?.Invoke(animation.fluentString); // Safe invoke
                        TriggerAnimation(animation, context, animation.parametersObjects);
                        break;
                    case AnimationState.Running:
                        yield return null;
                        break;
                    case AnimationState.End:
                        // animation has ended, if queue is empty, set state to none, else set state to ready
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
            Debug.Log($"Animation {context} ended, still {_animationsActive[context].queue.Count} animation in queue,state was {_animationsActive[context].state}");
            _animationsActive[context].state = AnimationState.End;
            Debug.Log($"state is {_animationsActive[context].state}");

            if (_activeGraphs.ContainsKey(context))
            {
                var graph = _activeGraphs[context];
                SimpleObjectPool.Instance.Return(graph);
                _activeGraphs.Remove(context);
            }
        }

        private void TriggerAnimation(AnimationQueueElement animationElement, string context, GameObject[] objects)
        {
            // Create a new instance of the graph using Object Pool
            var animationInstance = SimpleObjectPool.Instance.Get(animationElement.graphToClone);

            animationInstance.name = $"{context} === {animationElement.animationName}";
            _activeGraphs.Add(context, animationInstance);

            // Fetch the visualizer script from the pool instance
            var scriptVisualizer = animationInstance.GetComponent<IFluentVisualizer>();

            // Auto-attach if missing but we know the name
            if (scriptVisualizer == null && !string.IsNullOrEmpty(animationElement.scriptClassName))
            {
                System.Type type = null;
                foreach (var assembly in System.AppDomain.CurrentDomain.GetAssemblies())
                {
                    type = assembly.GetType("GeneratedVisualizers." + animationElement.scriptClassName);
                    if (type != null) break;
                }
                if (type != null)
                {
                    scriptVisualizer = animationInstance.AddComponent(type) as IFluentVisualizer;
                }
            }

            if (scriptVisualizer != null)
            {
                Debug.Log($"[AnimationsController] Using C# Script for {animationElement.animationName}");
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

            Debug.LogWarning($"No IFluentVisualizer found for {animationElement.animationName}. Animation will hang.");
            // Immediately end to avoid hang
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

            public GeTAtom value;

            public GameObject[] parametersObjects;

            public GameObject graphToClone;

            public string scriptClassName;

            public float duration;
        }
    }


}
