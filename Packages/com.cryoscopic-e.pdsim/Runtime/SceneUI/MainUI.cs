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
        Button nextStepButton;
        Button forwardAllButton;

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
        }

        private void OnEnable()
        {
            var uiDocument = GetComponent<UIDocument>();
            if (uiDocument == null)
            {
                Debug.LogError("[PDSim] UIDocument component not found on MainUI GameObject!");
                return;
            }

            var root = uiDocument.rootVisualElement;
            if (root == null)
            {
                Debug.LogError("[PDSim] rootVisualElement is null!");
                return;
            }

            root.styleSheets.Add(Resources.Load<StyleSheet>("SceneUI/SceneUSS"));
            BuildUI(root);

            // ── Wire up buttons ─────────────────────────────────────────────────
            playButton.clicked   += PlayButtonClicked;
            pauseButton.clicked  += PauseButtonClicked;
            reloadButton.clicked += ReloadButtonClicked;
            nextStepButton.clicked   += NextStepButtonClicked;
            forwardAllButton.clicked += ForwardAllButtonClicked;

            planPanelButton.clicked   += PlanPanelButtonClicked;
            actionTabButton.clicked   += ActionTabButtonClicked;
            objectInfoButton.clicked  += ObjectInfoButtonClicked;
            cameraControlsButton.clicked += CameraControlsButtonClicked;

            simulationSpeedControlsButton.clicked += () =>
                speedBar.style.display = speedBar.style.display == DisplayStyle.None
                    ? DisplayStyle.Flex : DisplayStyle.None;

            simulationSpeedSlider.SetValueWithoutNotify(1f);
            simulationSpeedSlider.RegisterValueChangedCallback(evt =>
                _controller.animationSpeed = evt.newValue);

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

            SetIdleState();
        }

        private void BuildUI(VisualElement root)
        {
            // ── Root container ──────────────────────────────────────────────────
            var rootContainer = new VisualElement { name = "Root" };
            rootContainer.AddToClassList("root-container");
            root.Add(rootContainer);

            // ── Header ──────────────────────────────────────────────────────────
            var header = new VisualElement { name = "Header" };
            header.AddToClassList("header");
            rootContainer.Add(header);

            // Top bar
            var topBar = new VisualElement { name = "TopBar" };
            topBar.AddToClassList("top-bar");
            header.Add(topBar);

            // Back button (not implemented — disabled)
            backButton = new Button { name = "BackButton" };
            backButton.AddToClassList("btn");
            backButton.AddToClassList("btn-icon");
            backButton.SetEnabled(false);
            var backIcon = new VisualElement();
            backIcon.AddToClassList("icon");
            backIcon.AddToClassList("icon--back");
            backButton.Add(backIcon);
            topBar.Add(backButton);

            // Simulation name
            var sceneName = SceneManager.GetActiveScene().name;
            var simulationNameLabel = new Label(char.ToUpper(sceneName[0]) + sceneName.Substring(1)) { name = "SimulationName" };
            simulationNameLabel.AddToClassList("sim-name");
            topBar.Add(simulationNameLabel);

            // PDSim branding
            var brandLabel = new Label("PDSim");
            brandLabel.AddToClassList("brand-label");
            topBar.Add(brandLabel);

            // ── Speed bar ───────────────────────────────────────────────────────
            speedBar = new VisualElement { name = "SpeedBar" };
            speedBar.AddToClassList("speed-bar");
            speedBar.style.display = DisplayStyle.Flex;
            header.Add(speedBar);

            simulationSpeedSlider = new Slider(0, 2) { name = "SpeedSlider", value = 1, focusable = false, showInputField = false };
            simulationSpeedSlider.AddToClassList("speed-slider");
            speedBar.Add(simulationSpeedSlider);

            var speedLabelsRow = new VisualElement { name = "SpeedLabels" };
            speedLabelsRow.AddToClassList("speed-labels-row");
            speedBar.Add(speedLabelsRow);

            speedLabelsRow.Add(new Label("0x") { name = "0x" }.WithClass("speed-label"));
            speedLabelsRow.Add(new Label("1x") { name = "1x" }.WithClass("speed-label"));
            speedLabelsRow.Add(new Label("2x") { name = "2x" }.WithClass("speed-label"));

            // ── Controls toolbar ────────────────────────────────────────────────
            var controls = new VisualElement { name = "Controls" };
            controls.AddToClassList("controls-bar");
            header.Add(controls);

            reloadButton    = AddIconButton(controls, "ReloadButton", "icon--restart");
            playButton      = AddIconButton(controls, "PlayButton", "icon--play");
            forwardAllButton = AddIconButton(controls, "PlayContButton", "icon--next");
            pauseButton     = AddIconButton(controls, "PauseButton", "icon--pause");
            nextStepButton  = AddIconButton(controls, "SkipButton", "icon--skip");

            // ── Footer (toolbox + timeline, pinned to bottom) ──────────────────
            var footer = new VisualElement { name = "Footer" };
            footer.AddToClassList("footer");
            rootContainer.Add(footer);

            // ── ToolBox ─────────────────────────────────────────────────────────
            var toolBox = new VisualElement { name = "ToolBox" };
            toolBox.AddToClassList("toolbox");
            footer.Add(toolBox);

            // Action info panel
            actionInfo = new VisualElement { name = "ActionInfo" };
            actionInfo.AddToClassList("action-info");
            actionInfo.style.display = DisplayStyle.None;
            toolBox.Add(actionInfo);

            actionStatus = new Label("Executing Action") { name = "Action" };
            actionStatus.AddToClassList("action-info__title");
            actionInfo.Add(actionStatus);

            predicateAnimated = new Label { name = "Predicate" };
            predicateAnimated.AddToClassList("action-info__subtitle");
            actionInfo.Add(predicateAnimated);

            // Buttons holder
            var buttonsHolder = new VisualElement { name = "ButtonsHolder" };
            buttonsHolder.AddToClassList("buttons-holder");
            toolBox.Add(buttonsHolder);

            planPanelButton = AddTextButton(buttonsHolder, "PlanPanelButton", "Plan Panel");
            actionTabButton = AddTextButton(buttonsHolder, "ActionTabButton", "Action Tab");
            simulationSpeedControlsButton = AddTextButton(buttonsHolder, "SpeedControlButton", "Speed Controls");
            objectInfoButton = AddTextButton(buttonsHolder, "ObjectInfoButton", "Object Info Panel");
            cameraControlsButton = AddTextButton(buttonsHolder, "CameraControlsButton", "Camera Controls");

            // Timeline progress bar
            timelineBar = new ProgressBar { name = "TimeLine", title = "Time Point: 0", value = 0 };
            timelineBar.AddToClassList("timeline-bar");
            footer.Add(timelineBar);

            // ── Camera hints overlay ────────────────────────────────────────────
            cameraHints = new VisualElement { name = "CameraHints" };
            cameraHints.AddToClassList("camera-hints");
            cameraHints.style.display = DisplayStyle.None;
            rootContainer.Add(cameraHints);

            var cameraHintsLabel = new Label("Press C to Control the Camera\n\nUse W,A,S,D to Move\nUse the Mouse to Rotate");
            cameraHintsLabel.AddToClassList("camera-hints__text");
            cameraHints.Add(cameraHintsLabel);
        }

        // ── UI builder helpers ──────────────────────────────────────────────────

        private Button AddIconButton(VisualElement parent, string name, string iconClass)
        {
            var button = new Button { name = name };
            button.AddToClassList("btn");
            button.AddToClassList("btn-icon");

            var icon = new VisualElement();
            icon.AddToClassList("icon");
            icon.AddToClassList(iconClass);
            button.Add(icon);

            parent.Add(button);
            return button;
        }

        private Button AddTextButton(VisualElement parent, string name, string text)
        {
            var button = new Button { name = name, text = text, focusable = false };
            button.AddToClassList("btn-text");
            parent.Add(button);
            return button;
        }

        private void OnDisable()
        {
            playButton.clicked   -= PlayButtonClicked;
            pauseButton.clicked  -= PauseButtonClicked;
            reloadButton.clicked -= ReloadButtonClicked;
            nextStepButton.clicked   -= NextStepButtonClicked;
            forwardAllButton.clicked -= ForwardAllButtonClicked;

            planPanelButton.clicked   -= PlanPanelButtonClicked;
            actionTabButton.clicked   -= ActionTabButtonClicked;
            objectInfoButton.clicked  -= ObjectInfoButtonClicked;
            cameraControlsButton.clicked -= CameraControlsButtonClicked;

            _controller.OnVisualisationReady       -= VisualisationReady;
            _controller.OnVisualiseInitBlock        -= VisualisationInitBlock;
            _controller.OnInitFluentStarted         -= InitFluentStarted;
            _controller.OnVisualisationActionBlock  -= VisualisationActionBlock;
            _controller.OnStepAnimationsComplete    -= StepAnimationsComplete;
            _controller.OnVisualisationFinished     -= VisualisationFinished;
            _controller.OnTimeLineAdvanced          -= TimelineAdvance;

            _animationsController.OnVisualisationStep -= VisualisationStep;

            _problemObjects.OnVisualisationObjectHovered   -= VisualisationObjectHovered;
            _problemObjects.OnVisualisationObjectUnhovered -= VisualisationObjectUnhovered;
        }

        // ── Button state machine ─────────────────────────────────────────────────

        private void SetIdleState()
        {
            playButton.SetEnabled(true);
            pauseButton.SetEnabled(false);
            nextStepButton.SetEnabled(false);
            forwardAllButton.SetEnabled(false);
        }

        private void SetAnimatingState()
        {
            playButton.SetEnabled(false);
            pauseButton.SetEnabled(true);
            nextStepButton.SetEnabled(false);
            forwardAllButton.SetEnabled(false);
        }

        private void SetWaitingStepState()
        {
            playButton.SetEnabled(false);
            pauseButton.SetEnabled(false);
            nextStepButton.SetEnabled(true);
            forwardAllButton.SetEnabled(true);
        }

        private void SetContinuousState()
        {
            playButton.SetEnabled(false);
            pauseButton.SetEnabled(true);
            nextStepButton.SetEnabled(false);
            forwardAllButton.SetEnabled(false);
        }

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
            _controller.SetContinuousMode(false);
            _controller.Pause();
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

    // ── Extension for fluent class assignment ────────────────────────────────
    internal static class VisualElementExtensions
    {
        public static T WithClass<T>(this T element, string className) where T : VisualElement
        {
            element.AddToClassList(className);
            return element;
        }
    }
}
