using GeTPlan.Core.Models; using GeTPlan.Core.Logic; using GeTPlan.Core.Models.Expressions; using PDSimAPI;
using PDSim.Components;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace PDSim.SceneUI
{
    /// <summary>
    /// Controls the main HUD.
    ///
    /// State machine:
    ///   Idle          – only Play is available
    ///   Animating     – animations are running; Pause available
    ///   WaitingStep   – step complete, user chooses Next Step or Forward All
    ///   Continuous    – auto-advancing; only Pause available
    ///   Finished      – plan complete; all playback controls disabled
    /// </summary>
    public class MainUI : MonoBehaviour
    {
        [SerializeField] PlanListUI PlanListUI;
        [SerializeField] StateListUI StateListUI;

        // ── Buttons ──────────────────────────────────────────────────────────────

        Button backButton;
        Button playButton;
        Button pauseButton;
        Button reloadButton;
        Button nextStepButton;      // was "SkipButton"
        Button forwardAllButton;    // was "PlayContButton"

        Button planPanelButton;
        Button actionTabButton;
        Button simulationSpeedControlsButton;
        Button objectInfoButton;
        Button cameraControlsButton;

        // ── Other elements ───────────────────────────────────────────────────────

        Slider simulationSpeedSlider;
        ProgressBar timelineBar;
        Label actionStatus;
        Label predicateAnimated;
        VisualElement actionInfo;
        VisualElement speedBar;
        VisualElement cameraHints;

        // ── Internal ─────────────────────────────────────────────────────────────

        private Controller _controller;
        private AnimationsController _animationsController;
        private ProblemObjects _problemObjects;

        // ── Lifecycle ────────────────────────────────────────────────────────────

        private void Awake()
        {
            _controller           = Controller.Instance;
            _problemObjects       = ProblemObjects.Instance;
            _animationsController = AnimationsController.Instance;

            if (_controller == null) Debug.LogError("[PDSim] Controller.Instance is null!");
            if (_problemObjects == null) Debug.LogError("[PDSim] ProblemObjects.Instance is null!");
            if (_animationsController == null) Debug.LogError("[PDSim] AnimationsController.Instance is null!");

            var uiDocument = GetComponent<UIDocument>();
            if (uiDocument == null)
            {
                Debug.LogError("[PDSim] UIDocument component not found on MainUI GameObject!");
                return;
            }

            if (uiDocument.visualTreeAsset == null)
            {
                Debug.Log("[PDSim] visualTreeAsset is null, attempting to load from Resources...");
                uiDocument.visualTreeAsset = Resources.Load<VisualTreeAsset>("SceneUI/VisualisationUI");
                if (uiDocument.visualTreeAsset == null)
                {
                    Debug.LogError("[PDSim] Failed to load VisualisationUI from Resources/SceneUI/VisualisationUI");
                }
            }

            var root = uiDocument.rootVisualElement;
            if (root == null)
            {
                Debug.LogError("[PDSim] rootVisualElement is null!");
                return;
            }

            backButton         = root.Q<Button>("BackButton");
            playButton         = root.Q<Button>("PlayButton");
            pauseButton        = root.Q<Button>("PauseButton");
            reloadButton       = root.Q<Button>("ReloadButton");
            nextStepButton     = root.Q<Button>("SkipButton");
            forwardAllButton   = root.Q<Button>("PlayContButton");

            planPanelButton              = root.Q<Button>("PlanPanelButton");
            actionTabButton              = root.Q<Button>("ActionTabButton");
            objectInfoButton             = root.Q<Button>("ObjectInfoButton");
            cameraControlsButton         = root.Q<Button>("CameraControlsButton");
            simulationSpeedControlsButton = root.Q<Button>("SpeedControlButton");

            simulationSpeedSlider = root.Q<Slider>("SpeedSlider");
            timelineBar           = root.Q<ProgressBar>("TimeLine");

            actionInfo = root.Q<VisualElement>("ActionInfo");
            cameraHints = root.Q<VisualElement>("CameraHints");
            speedBar = root.Q<VisualElement>("SpeedBar");

            // Log missing elements
            if (backButton == null) Debug.LogError("[PDSim] Missing UI Element: BackButton");
            if (playButton == null) Debug.LogError("[PDSim] Missing UI Element: PlayButton");
            if (pauseButton == null) Debug.LogError("[PDSim] Missing UI Element: PauseButton");
            if (reloadButton == null) Debug.LogError("[PDSim] Missing UI Element: ReloadButton");
            if (nextStepButton == null) Debug.LogError("[PDSim] Missing UI Element: SkipButton");
            if (forwardAllButton == null) Debug.LogError("[PDSim] Missing UI Element: PlayContButton");
            if (planPanelButton == null) Debug.LogError("[PDSim] Missing UI Element: PlanPanelButton");
            if (actionTabButton == null) Debug.LogError("[PDSim] Missing UI Element: ActionTabButton");
            if (objectInfoButton == null) Debug.LogError("[PDSim] Missing UI Element: ObjectInfoButton");
            if (cameraControlsButton == null) Debug.LogError("[PDSim] Missing UI Element: CameraControlsButton");
            if (simulationSpeedControlsButton == null) Debug.LogError("[PDSim] Missing UI Element: SpeedControlButton");
            if (simulationSpeedSlider == null) Debug.LogError("[PDSim] Missing UI Element: SpeedSlider");
            if (timelineBar == null) Debug.LogError("[PDSim] Missing UI Element: TimeLine");
            if (actionInfo == null) Debug.LogError("[PDSim] Missing UI Element: ActionInfo");
            if (cameraHints == null) Debug.LogError("[PDSim] Missing UI Element: CameraHints");
            if (speedBar == null) Debug.LogError("[PDSim] Missing UI Element: SpeedBar");

            // Scene name label
            var simulationNameLabel = root.Q<Label>("SimulationName");
            if (simulationNameLabel == null) Debug.LogError("[PDSim] Missing UI Element: SimulationName");
            var sceneName = SceneManager.GetActiveScene().name;
            if (simulationNameLabel != null)
                simulationNameLabel.text = char.ToUpper(sceneName[0]) + sceneName.Substring(1);

            actionStatus       = root.Q<Label>("Action");
            predicateAnimated  = root.Q<Label>("Predicate");
            if (actionStatus == null) Debug.LogError("[PDSim] Missing UI Element: Action (Label)");
            if (predicateAnimated == null) Debug.LogError("[PDSim] Missing UI Element: Predicate (Label)");

            // ── Wire up buttons ─────────────────────────────────────────────────
            if (backButton != null) backButton.clicked   += BackButtonClicked;
            if (playButton != null) playButton.clicked   += PlayButtonClicked;
            if (pauseButton != null) pauseButton.clicked  += PauseButtonClicked;
            if (reloadButton != null) reloadButton.clicked += ReloadButtonClicked;
            if (nextStepButton != null) nextStepButton.clicked   += NextStepButtonClicked;
            if (forwardAllButton != null) forwardAllButton.clicked += ForwardAllButtonClicked;

            if (planPanelButton != null) planPanelButton.clicked   += PlanPanelButtonClicked;
            if (actionTabButton != null) actionTabButton.clicked   += ActionTabButtonClicked;
            if (objectInfoButton != null) objectInfoButton.clicked  += ObjectInfoButtonClicked;
            if (cameraControlsButton != null) cameraControlsButton.clicked += CameraControlsButtonClicked;

            if (simulationSpeedControlsButton != null)
            {
                simulationSpeedControlsButton.clicked += () =>
                    speedBar.style.display = speedBar.style.display == DisplayStyle.None
                        ? DisplayStyle.Flex : DisplayStyle.None;
            }

            // Speed slider controls animation speed, not Time.timeScale, so it
            // doesn't interfere with the pause mechanism.
            if (simulationSpeedSlider != null)
            {
                simulationSpeedSlider.SetValueWithoutNotify(1f);
                simulationSpeedSlider.RegisterValueChangedCallback(evt =>
                    _controller.animationSpeed = evt.newValue);
            }

            // ── Subscribe to Controller events ──────────────────────────────────
            _controller.OnVisualisationReady       += VisualisationReady;
            _controller.OnVisualiseInitBlock        += VisualisationInitBlock;
            _controller.OnInitFluentStarted         += InitFluentStarted;
            _controller.OnVisualisationActionBlock  += VisualisationActionBlock;
            _controller.OnStepAnimationsComplete    += StepAnimationsComplete;
            _controller.OnVisualisationFinished     += VisualisationFinished;
            _controller.OnTimeLineAdvanced          += TimelineAdvance;

            _animationsController.OnVisualisationStep += VisualisationStep;

            _problemObjects.OnVisualisationObjectHovered   += VisualisationObjectHovered;
            _problemObjects.OnVisualisationObjectUnhovered += VisualisationObjectUnhovered;

            // Start in Idle state
            SetIdleState();
        }

        private void OnDisable()
        {
            backButton.clicked   -= BackButtonClicked;
            playButton.clicked   -= PlayButtonClicked;
            pauseButton.clicked  -= PauseButtonClicked;
            reloadButton.clicked -= ReloadButtonClicked;
            nextStepButton.clicked   -= NextStepButtonClicked;
            forwardAllButton.clicked -= ForwardAllButtonClicked;

            planPanelButton.clicked   -= PlanPanelButtonClicked;
            actionTabButton.clicked   -= ActionTabButtonClicked;
            objectInfoButton.clicked  -= ObjectInfoButtonClicked;
            cameraControlsButton.clicked -= CameraControlsButtonClicked;

            _controller.OnInitFluentStarted -= InitFluentStarted;
        }

        // ── Button state machine ─────────────────────────────────────────────────

        /// <summary>Before Play is pressed.</summary>
        private void SetIdleState()
        {
            playButton.SetEnabled(true);
            pauseButton.SetEnabled(false);
            nextStepButton.SetEnabled(false);
            forwardAllButton.SetEnabled(false);
        }

        /// <summary>Animations are playing (init block or a single step).</summary>
        private void SetAnimatingState()
        {
            playButton.SetEnabled(false);
            pauseButton.SetEnabled(true);
            nextStepButton.SetEnabled(false);
            forwardAllButton.SetEnabled(false);
        }

        /// <summary>A step finished; waiting for the user to choose next action.</summary>
        private void SetWaitingStepState()
        {
            playButton.SetEnabled(false);
            pauseButton.SetEnabled(false);
            nextStepButton.SetEnabled(true);
            forwardAllButton.SetEnabled(true);
        }

        /// <summary>Auto-advancing through all remaining steps.</summary>
        private void SetContinuousState()
        {
            playButton.SetEnabled(false);
            pauseButton.SetEnabled(true);
            nextStepButton.SetEnabled(false);
            forwardAllButton.SetEnabled(false);
        }

        /// <summary>Plan fully visualised.</summary>
        private void SetFinishedState()
        {
            playButton.SetEnabled(false);
            pauseButton.SetEnabled(false);
            nextStepButton.SetEnabled(false);
            forwardAllButton.SetEnabled(false);
        }

        // ── Button handlers ──────────────────────────────────────────────────────

        private void PlayButtonClicked()
        {
            _controller.StartVisualisation();
            SetAnimatingState();
        }

        private void PauseButtonClicked()
        {
            // Stop continuous mode; current animation step plays to completion,
            // then OnStepAnimationsComplete will move us to WaitingStep.
            _controller.SetContinuousMode(false);
            _controller.Pause();
            // Disable Pause immediately so the user gets feedback.
            pauseButton.SetEnabled(false);
        }

        private void NextStepButtonClicked()
        {
            if (_controller.IsPaused) _controller.Resume();
            SetAnimatingState();
            _controller.Advance();
        }

        private void ForwardAllButtonClicked()
        {
            if (_controller.IsPaused) _controller.Resume();
            SetContinuousState();
            _controller.SetContinuousMode(true);
        }

        private void ReloadButtonClicked()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        private void BackButtonClicked()
        {
            throw new System.NotImplementedException("Back to Menu");
        }

        // ── Controller event handlers ────────────────────────────────────────────

        private void VisualisationReady(List<GroundedAction> planList)
        {
            actionStatus.text     = "Ready";
            predicateAnimated.text = "";
            PlanListUI.InitializePlanList(planList);
        }

        private void VisualisationInitBlock()
        {
            actionStatus.text      = "Init";
            predicateAnimated.text = "";
        }

        private void InitFluentStarted(string fluent, int index, int total)
        {
            actionStatus.text      = $"Init  [{index + 1} / {total}]";
            predicateAnimated.text = fluent;
        }

        private void VisualisationActionBlock(string actionName, int index)
        {
            actionStatus.text = actionName;
            PlanListUI.HighlightCurrentAction(index);
        }

        private void VisualisationStep(string fluent)
        {
            predicateAnimated.text = fluent;
        }

        /// <summary>
        /// Called once per completed step when not in continuous mode.
        /// Moves to WaitingStep so the user can choose what to do next.
        /// </summary>
        private void StepAnimationsComplete()
        {
            SetWaitingStepState();
            predicateAnimated.text = "";
        }

        private void VisualisationFinished()
        {
            SetFinishedState();
            actionStatus.text      = "Simulation Finished";
            predicateAnimated.text = "";
        }

        private void TimelineAdvance(double time, double progress)
        {
            timelineBar.title = $"Time Point: {time}";
            timelineBar.SetValueWithoutNotify((float)(progress * 100f));
        }

        private void VisualisationObjectHovered(VisualisationObject simObject)
        {
            StateListUI.InitializeList(simObject);
        }

        private void VisualisationObjectUnhovered()
        {
            StateListUI.Clear();
        }

        // ── Panel toggles ────────────────────────────────────────────────────────

        private void PlanPanelButtonClicked() => PlanListUI.ToggleVisibility();

        private void ActionTabButtonClicked() =>
            actionInfo.style.display = actionInfo.style.display == DisplayStyle.None
                ? DisplayStyle.Flex : DisplayStyle.None;

        private void ObjectInfoButtonClicked() => StateListUI.ToggleVisibility();

        private void CameraControlsButtonClicked()
        {
            cameraHints.style.display = DisplayStyle.Flex;
            cameraHints.schedule.Execute(() => cameraHints.style.display = DisplayStyle.None)
                       .StartingIn(3000);
        }
    }
}
