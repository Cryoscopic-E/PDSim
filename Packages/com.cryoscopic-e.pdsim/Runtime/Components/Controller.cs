using GeTModel;
using PDSim.ScriptableObjects;
using PDSimAPI;
using System.Collections.Generic;
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

        // Data Assets (Problem model, plan, etc.)
        // ---------------------------------------


        public PlanningProblem problem;
        public PlanGeneration planGeneration;

        public Visualisation visualisation;

        [Header("Settings")]
        [Range(0.1f, 5f)]
        public float animationSpeed = 1.0f;
        public bool autoAdvance = true;

        public bool IsPaused => visualisation != null && visualisation.VisState.IsPaused;



        private ProblemObjects _problemObjects;
        private InitBlock _initBlock;
        private TypeHierarchy _typeHierarchy;

        private AnimationsController _animationsController;

        // Events and Delegates
        // --------------------


        // OnVisualiseInitBlock is called when the visualisation is animating the initial state
        public delegate void VisualiseInitBlock();
        public event VisualiseInitBlock OnVisualiseInitBlock;

        // OnVisualisationActionBlock is called when the visualisation is animating an action
        public delegate void VisualisationtionActionBlock(string block, int i);
        public event VisualisationtionActionBlock OnVisualisationActionBlock;

        // OnVisualisationReady is called when the visualisation is ready to start
        public delegate void VisualisationReady(List<GeTActionInstance> actions);
        public event VisualisationReady OnVisualisationReady;

        // OnVisualisationFinished is called when the visualisation is finished
        public delegate void VisualisationFinished();
        public event VisualisationFinished OnVisualisationFinished;

        // OnVisualisationFinished is called when the visualisation is finished
        public delegate void TimeLineAdvanced(double time, double progress);
        public event TimeLineAdvanced OnTimeLineAdvanced;

        private void Awake()
        {
            _problemObjects = ProblemObjects.Instance;
            _initBlock = InitBlock.Instance;
            _typeHierarchy = TypeHierarchy.Instance;

            _animationsController = GetComponent<AnimationsController>();
        }


        private void Start()
        {
            if (problem == null || planGeneration == null)
            {
                return;
            }

            visualisation = new Visualisation(problem.proto, planGeneration.proto);


            visualisation.TimeLineAdvance += (sender, args) =>
            {
                OnTimeLineAdvanced(args.Time.ToDouble(), args.Progress);
            };

            visualisation.WorldStateChanged += (sender, args) =>
            {
                _animationsController.UpdateQueue(args.AppliedAction, args.NewStateVar);
            };
            // visualisation.VisualisationStart += (sender, args) =>
            // {
            //     Debug.Log("Visualisation started");
            // };

            visualisation.ActionStarted += (sender, action) =>
            {
                int index = visualisation.PlanGeneration.Plan.Actions.IndexOf(action);
                OnVisualisationActionBlock?.Invoke(action.ToString(), index);
            };

            visualisation.VisualisationEnd += (sender, args) =>
            {
                OnVisualisationFinished();
            };
            AnimationsController.Instance.OnTimePointAnimationEnd += () =>
            {
                if (autoAdvance)
                {
                    visualisation.Advance();
                }
            };
        }

        public void StartVisualisation()
        {
            OnVisualisationReady(visualisation.PlanGeneration.Plan.Actions);
            // RefreshAnimations the queue with the initial state
            foreach (var fluent in visualisation.CurrentWorldState.State)
            {
                _animationsController.UpdateQueue(null, fluent);
            }
            OnVisualiseInitBlock();
        }

        public void Pause()
        {
            visualisation.Pause();
        }

        public void Resume()
        {
            visualisation.Resume();
        }

        public void AdvanceContinuously()
        {
            if (visualisation.VisState.IsRunning)
            {
                visualisation.Advance();
            }
            else
            {
                visualisation.Resume();
            }
        }

        internal void Advance()
        {
            visualisation.Advance();
        }
    }
}