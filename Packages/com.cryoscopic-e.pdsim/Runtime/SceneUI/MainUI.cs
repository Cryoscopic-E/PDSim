using GeTPlan.Core.Models;
using PDSim.Components;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace PDSim.SceneUI
{
    /// <summary>
    /// Controls the main HUD of the simulation.
    /// Manages playback controls, speed, and visibility of various panels.
    /// </summary>
    public class MainUI : MonoBehaviour
    {
        #region Serialized Fields
        [Header("UI Dependencies")]
        [Tooltip("Reference to the Plan List UI component")]
        [SerializeField] private PlanListUI PlanListUI;
        [Tooltip("Reference to the State List UI component")]
        [SerializeField] private StateListUI StateListUI;
        #endregion

        #region Private Fields
        // UI Button references for controlling simulation playback and panel visibility.

        private Button _backButton;
        private Button _playButton;
        private Button _pauseButton;
        private Button _reloadButton;
        private Button _nextStepButton;
        private Button _forwardAllButton;

        private Button _planPanelButton;
        private Button _actionTabButton;
        private Button _simulationSpeedControlsButton;
        private Button _objectInfoButton;
        private Button _cameraControlsButton;

        // Miscellaneous UI elements including sliders, progress bars, and labels for status display.

        private Slider _simulationSpeedSlider;
        private ProgressBar _timelineBar;
        private Label _actionStatus;
        private Label _predicateAnimated;
        private VisualElement _actionInfo;
        private VisualElement _speedBar;
        private VisualElement _cameraHints;

        // Internal state and references to simulation controllers.

        private Controller _controller;
        private AnimationsController _animationsController;
        private ProblemObjects _problemObjects;
        #endregion

        #region Unity Lifecycle
        private void Awake()
        {
            _controller = Controller.Instance;
            _problemObjects = ProblemObjects.Instance;
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

            // Setup event listeners for the playback and navigation buttons.
            _playButton.clicked += PlayButtonClicked;
            _pauseButton.clicked += PauseButtonClicked;
            _reloadButton.clicked += ReloadButtonClicked;
            _nextStepButton.clicked += NextStepButtonClicked;
            _forwardAllButton.clicked += ForwardAllButtonClicked;

            _planPanelButton.clicked += PlanPanelButtonClicked;
            _actionTabButton.clicked += ActionTabButtonClicked;
            _objectInfoButton.clicked += ObjectInfoButtonClicked;
            _cameraControlsButton.clicked += CameraControlsButtonClicked;

            _simulationSpeedControlsButton.clicked += () =>
                _speedBar.style.display = _speedBar.style.display == DisplayStyle.None
                    ? DisplayStyle.Flex : DisplayStyle.None;

            _simulationSpeedSlider.SetValueWithoutNotify(1f);
            _simulationSpeedSlider.RegisterValueChangedCallback(evt =>
                _controller.AnimationSpeed = evt.newValue);

            // Listen for simulation and animation lifecycle events from the controllers.
            _controller.OnVisualisationReady += VisualisationReady;
            _controller.OnVisualiseInitBlock += VisualisationInitBlock;
            _controller.OnInitFluentStarted += InitFluentStarted;
            _controller.OnVisualisationActionBlock += VisualisationActionBlock;
            _controller.OnStepAnimationsComplete += StepAnimationsComplete;
            _controller.OnVisualisationFinished += VisualisationFinished;
            _controller.OnTimeLineAdvanced += TimelineAdvance;

            _animationsController.OnVisualisationStep += VisualisationStep;

            _problemObjects.OnVisualisationObjectHovered += VisualisationObjectHovered;
            _problemObjects.OnVisualisationObjectUnhovered += VisualisationObjectUnhovered;

            SetIdleState();
        }

        private void OnDisable()
        {
            _playButton.clicked -= PlayButtonClicked;
            _pauseButton.clicked -= PauseButtonClicked;
            _reloadButton.clicked -= ReloadButtonClicked;
            _nextStepButton.clicked -= NextStepButtonClicked;
            _forwardAllButton.clicked -= ForwardAllButtonClicked;

            _planPanelButton.clicked -= PlanPanelButtonClicked;
            _actionTabButton.clicked -= ActionTabButtonClicked;
            _objectInfoButton.clicked -= ObjectInfoButtonClicked;
            _cameraControlsButton.clicked -= CameraControlsButtonClicked;

            _controller.OnVisualisationReady -= VisualisationReady;
            _controller.OnVisualiseInitBlock -= VisualisationInitBlock;
            _controller.OnInitFluentStarted -= InitFluentStarted;
            _controller.OnVisualisationActionBlock -= VisualisationActionBlock;
            _controller.OnStepAnimationsComplete -= StepAnimationsComplete;
            _controller.OnVisualisationFinished -= VisualisationFinished;
            _controller.OnTimeLineAdvanced -= TimelineAdvance;

            _animationsController.OnVisualisationStep -= VisualisationStep;

            _problemObjects.OnVisualisationObjectHovered -= VisualisationObjectHovered;
            _problemObjects.OnVisualisationObjectUnhovered -= VisualisationObjectUnhovered;
        }
        #endregion

        #region UI Building
        private void BuildUI(VisualElement root)
        {
            // Create the main container that holds all UI elements.
            var rootContainer = new VisualElement { name = "Root" };
            rootContainer.AddToClassList("root-container");
            root.Add(rootContainer);

            // Build the header section including the top bar and branding.
            var header = new VisualElement { name = "Header" };
            header.AddToClassList("header");
            rootContainer.Add(header);

            // Top bar
            var topBar = new VisualElement { name = "TopBar" };
            topBar.AddToClassList("top-bar");
            header.Add(topBar);

            // Back button (not implemented — disabled)
            _backButton = new Button { name = "BackButton" };
            _backButton.AddToClassList("btn");
            _backButton.AddToClassList("btn-icon");
            _backButton.SetEnabled(false);
            var backIcon = new VisualElement();
            backIcon.AddToClassList("icon");
            backIcon.AddToClassList("icon--back");
            _backButton.Add(backIcon);
            topBar.Add(_backButton);

            // Simulation name
            var sceneName = SceneManager.GetActiveScene().name;
            var simulationNameLabel = new Label(char.ToUpper(sceneName[0]) + sceneName.Substring(1)) { name = "SimulationName" };
            simulationNameLabel.AddToClassList("sim-name");
            topBar.Add(simulationNameLabel);

            // PDSim branding
            var brandLabel = new Label("PDSim");
            brandLabel.AddToClassList("brand-label");
            topBar.Add(brandLabel);

            // Build the simulation speed control section.
            _speedBar = new VisualElement { name = "SpeedBar" };
            _speedBar.AddToClassList("speed-bar");
            _speedBar.style.display = DisplayStyle.Flex;
            header.Add(_speedBar);

            _simulationSpeedSlider = new Slider(0, 2) { name = "SpeedSlider", value = 1, focusable = false, showInputField = false };
            _simulationSpeedSlider.AddToClassList("speed-slider");
            _speedBar.Add(_simulationSpeedSlider);

            var speedLabelsRow = new VisualElement { name = "SpeedLabels" };
            speedLabelsRow.AddToClassList("speed-labels-row");
            _speedBar.Add(speedLabelsRow);

            speedLabelsRow.Add(new Label("0x") { name = "0x" }.WithClass("speed-label"));
            speedLabelsRow.Add(new Label("1x") { name = "1x" }.WithClass("speed-label"));
            speedLabelsRow.Add(new Label("2x") { name = "2x" }.WithClass("speed-label"));

            // Build the main playback control toolbar.
            var controls = new VisualElement { name = "Controls" };
            controls.AddToClassList("controls-bar");
            header.Add(controls);

            _reloadButton = AddIconButton(controls, "ReloadButton", "icon--restart");
            _playButton = AddIconButton(controls, "PlayButton", "icon--play");
            _forwardAllButton = AddIconButton(controls, "PlayContButton", "icon--next");
            _pauseButton = AddIconButton(controls, "PauseButton", "icon--pause");
            _nextStepButton = AddIconButton(controls, "SkipButton", "icon--skip");

            // Build the footer section containing the toolbox and timeline.
            var footer = new VisualElement { name = "Footer" };
            footer.AddToClassList("footer");
            rootContainer.Add(footer);

            // Build the toolbox containing action information and toggle buttons.
            var toolBox = new VisualElement { name = "ToolBox" };
            toolBox.AddToClassList("toolbox");
            footer.Add(toolBox);

            // Action info panel
            _actionInfo = new VisualElement { name = "ActionInfo" };
            _actionInfo.AddToClassList("action-info");
            _actionInfo.style.display = DisplayStyle.None;
            toolBox.Add(_actionInfo);

            _actionStatus = new Label("Executing Action") { name = "Action" };
            _actionStatus.AddToClassList("action-info__title");
            _actionInfo.Add(_actionStatus);

            _predicateAnimated = new Label { name = "Predicate" };
            _predicateAnimated.AddToClassList("action-info__subtitle");
            _actionInfo.Add(_predicateAnimated);

            // Buttons holder
            var buttonsHolder = new VisualElement { name = "ButtonsHolder" };
            buttonsHolder.AddToClassList("buttons-holder");
            toolBox.Add(buttonsHolder);

            _planPanelButton = AddTextButton(buttonsHolder, "PlanPanelButton", "Plan Panel");
            _actionTabButton = AddTextButton(buttonsHolder, "ActionTabButton", "Action Tab");
            _simulationSpeedControlsButton = AddTextButton(buttonsHolder, "SpeedControlButton", "Speed Controls");
            _objectInfoButton = AddTextButton(buttonsHolder, "ObjectInfoButton", "Object Info Panel");
            _cameraControlsButton = AddTextButton(buttonsHolder, "CameraControlsButton", "Camera Controls");

            // Timeline progress bar
            _timelineBar = new ProgressBar { name = "TimeLine", title = "Time Point: 0", value = 0 };
            _timelineBar.AddToClassList("timeline-bar");
            footer.Add(_timelineBar);

            // Build the overlay that provides camera navigation hints.
            _cameraHints = new VisualElement { name = "CameraHints" };
            _cameraHints.AddToClassList("camera-hints");
            _cameraHints.style.display = DisplayStyle.None;
            rootContainer.Add(_cameraHints);

            var cameraHintsLabel = new Label("Press C to Control the Camera\n\nUse W,A,S,D to Move\nUse the Mouse to Rotate");
            cameraHintsLabel.AddToClassList("camera-hints__text");
            _cameraHints.Add(cameraHintsLabel);
        }

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
        #endregion

        #region Button State Management
        private void SetIdleState()
        {
            _playButton.SetEnabled(true);
            _pauseButton.SetEnabled(false);
            _nextStepButton.SetEnabled(false);
            _forwardAllButton.SetEnabled(false);
        }

        private void SetAnimatingState()
        {
            _playButton.SetEnabled(false);
            _pauseButton.SetEnabled(true);
            _nextStepButton.SetEnabled(false);
            _forwardAllButton.SetEnabled(false);
        }

        private void SetWaitingStepState()
        {
            _playButton.SetEnabled(false);
            _pauseButton.SetEnabled(false);
            _nextStepButton.SetEnabled(true);
            _forwardAllButton.SetEnabled(true);
        }

        private void SetContinuousState()
        {
            _playButton.SetEnabled(false);
            _pauseButton.SetEnabled(true);
            _nextStepButton.SetEnabled(false);
            _forwardAllButton.SetEnabled(false);
        }

        private void SetFinishedState()
        {
            _playButton.SetEnabled(false);
            _pauseButton.SetEnabled(false);
            _nextStepButton.SetEnabled(false);
            _forwardAllButton.SetEnabled(false);
        }
        #endregion

        #region Button Handlers
        private void PlayButtonClicked()
        {
            _controller.StartVisualisation();
            SetWaitingStepState();
        }

        private void PauseButtonClicked()
        {
            _controller.SetContinuousMode(false);
            _controller.Pause();
            _pauseButton.SetEnabled(false);
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
        #endregion

        #region Controller Event Handlers
        private void VisualisationReady(List<GroundedAction> planList)
        {
            _actionStatus.text = "Ready";
            _predicateAnimated.text = "";
            PlanListUI.InitializePlanList(planList);
        }

        private void VisualisationInitBlock()
        {
            _actionStatus.text = "Init";
            _predicateAnimated.text = "";
        }

        private void InitFluentStarted(string fluent, int index, int total)
        {
            _actionStatus.text = $"Init  [{index + 1} / {total}]";
            _predicateAnimated.text = fluent;
        }

        private void VisualisationActionBlock(string actionName, int index)
        {
            _actionStatus.text = actionName;
            PlanListUI.HighlightCurrentAction(index);
        }

        private void VisualisationStep(string fluent)
        {
            _predicateAnimated.text = fluent;
        }

        private void StepAnimationsComplete()
        {
            SetWaitingStepState();
            _predicateAnimated.text = "";
        }

        private void VisualisationFinished()
        {
            SetFinishedState();
            _actionStatus.text = "Simulation Finished";
            _predicateAnimated.text = "";
        }

        private void TimelineAdvance(double time, double progress)
        {
            _timelineBar.title = $"Time Point: {time}";
            _timelineBar.SetValueWithoutNotify((float)(progress * 100f));
        }

        private void VisualisationObjectHovered(VisualisationObject simObject)
        {
            StateListUI.InitializeList(simObject);
        }

        private void VisualisationObjectUnhovered()
        {
            StateListUI.Clear();
        }
        #endregion

        #region Panel Toggles
        private void PlanPanelButtonClicked() => PlanListUI.ToggleVisibility();

        private void ActionTabButtonClicked() =>
            _actionInfo.style.display = _actionInfo.style.display == DisplayStyle.None
                ? DisplayStyle.Flex : DisplayStyle.None;

        private void ObjectInfoButtonClicked() => StateListUI.ToggleVisibility();

        private void CameraControlsButtonClicked()
        {
            _cameraHints.style.display = DisplayStyle.Flex;
            _cameraHints.schedule.Execute(() => _cameraHints.style.display = DisplayStyle.None)
                       .StartingIn(3000);
        }
        #endregion
    }

    // Helper extensions to simplify visual element configuration.
    internal static class VisualElementExtensions
    {
        public static T WithClass<T>(this T element, string className) where T : VisualElement
        {
            element.AddToClassList(className);
            return element;
        }
    }
}
