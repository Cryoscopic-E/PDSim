using GeTModel;
using PDSim.Components;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace PDSim.SceneUI
{
    public class MainUI : MonoBehaviour
    {

        [SerializeField]
        PlanListUI PlanListUI;

        [SerializeField]
        StateListUI StateListUI;

        Button backButton;

        Button playButton;
        Button pauseButton;
        Button reloadButton;
        Button playContinuoslyButton;
        Button nextTimePointButton;

        Button planPanelButton;
        Button actionTabButton;
        Button simulationSpeedControlsButton;
        Button objectInfoButton;
        Button cameraControlsButton;

        Slider simulationSpeedSlider;

        ProgressBar timelineBar;

        Label actionStatus;
        Label predicateAnimated;

        VisualElement actionInfo;

        VisualElement speedBar;

        VisualElement cameraHints;

        private Controller _controller;

        private AnimationsController _animationsController;

        private ProblemObjects _problemObjects;

        private void Awake()
        {
            _controller = Controller.Instance;
            _problemObjects = ProblemObjects.Instance;
            _animationsController = AnimationsController.Instance;

            var root = GetComponent<UIDocument>().rootVisualElement;

            backButton = root.Q<Button>("BackButton");
            playButton = root.Q<Button>("PlayButton");

            pauseButton = root.Q<Button>("PauseButton");
            pauseButton.SetEnabled(false);

            reloadButton = root.Q<Button>("ReloadButton");

            playContinuoslyButton = root.Q<Button>("PlayContButton");
            nextTimePointButton = root.Q<Button>("SkipButton");

            timelineBar = root.Q<ProgressBar>("TimeLine");

            planPanelButton = root.Q<Button>("PlanPanelButton");
            actionTabButton = root.Q<Button>("ActionTabButton");
            objectInfoButton = root.Q<Button>("ObjectInfoButton");
            cameraControlsButton = root.Q<Button>("CameraControlsButton");

            simulationSpeedSlider = root.Q<Slider>("SpeedSlider");

            simulationSpeedControlsButton = root.Q<Button>("SpeedControlButton");

            actionInfo = root.Q<VisualElement>("ActionInfo");
            actionInfo.style.display = DisplayStyle.None;

            cameraHints = root.Q<VisualElement>("CameraHints");
            cameraHints.style.display = DisplayStyle.None;


            backButton.clicked += BackButtonClicked;

            playButton.clicked += PlayButtonClicked;

            pauseButton.clicked += PauseButtonClicked;

            reloadButton.clicked += ReloadButtonClicked;
            playContinuoslyButton.clicked += PlayContinuouslyButtonClicked;
            nextTimePointButton.clicked += NextTimePointButtonClicked;

            speedBar = root.Q<VisualElement>("SpeedBar");
            speedBar.style.display = DisplayStyle.None;

            simulationSpeedControlsButton.clicked += () =>
            {
                speedBar.style.display = speedBar.style.display == DisplayStyle.None ? DisplayStyle.Flex : DisplayStyle.None;
            };

            planPanelButton.clicked += PlanPanelButtonClicked;
            actionTabButton.clicked += ActionTabButtonClicked;
            objectInfoButton.clicked += ObjectInfoButtonClicked;
            cameraControlsButton.clicked += CameraControlsButtonClicked;

            simulationSpeedSlider.SetValueWithoutNotify(1);
            simulationSpeedSlider.RegisterValueChangedCallback((evt) =>
            {
                Time.timeScale = evt.newValue;
            });


            var simulationNameLabel = root.Q<Label>("SimulationName");
            var sceneName = SceneManager.GetActiveScene().name;
            simulationNameLabel.text = char.ToUpper(sceneName[0]) + sceneName.Substring(1);


            actionStatus = root.Q<Label>("Action");
            predicateAnimated = root.Q<Label>("Predicate");


            // Simulation Manager Events
            _controller.OnVisualisationReady += VisualisationReady;
            _controller.OnVisualiseInitBlock += VisualisationInitBlock;
            _controller.OnVisualisationActionBlock += VisualisationActionBlock;
            _controller.OnVisualisationFinished += VisualisationFinished;
            _controller.OnTimeLineAdvanced += TimelineAdvance;


            _animationsController.OnVisualisationStep += VisualisationStep;


            _problemObjects.OnVisualisationObjectHovered += VisualisationObjectHovered;
            _problemObjects.OnVisualisationObjectUnhovered += VisualisationObjectUnhovered;

        }

        private void TimelineAdvance(double time, double progress)
        {
            timelineBar.title = $"Time Point: {time.ToString()}";
            timelineBar.SetValueWithoutNotify((float)(progress * 100f));
        }

        private void VisualisationReady(List<GeTActionInstance> planList)
        {
            actionStatus.text = "Ready";
            predicateAnimated.text = "";
            PlanListUI.InitializePlanList(planList);
        }

        private void VisualisationActionBlock(string actionName, int index)
        {
            actionStatus.text = actionName;
            PlanListUI.HighlightCurrentAction(index);
        }

        private void VisualisationInitBlock()
        {
            actionStatus.text = "Init Block";
        }

        private void VisualisationStep(string fluent)
        {
            predicateAnimated.text = fluent;
        }

        private void VisualisationFinished()
        {
            playButton.SetEnabled(false);
            pauseButton.SetEnabled(false);
            actionStatus.text = "Simulation Finished";
            predicateAnimated.text = "";
        }

        private void VisualisationObjectHovered(VisualisationObject simObject)
        {
            StateListUI.InitializeList(simObject);
        }

        private void VisualisationObjectUnhovered()
        {
            StateListUI.Clear();
        }


        private void OnDisable()
        {
            backButton.clicked -= BackButtonClicked;
            playButton.clicked -= PlayButtonClicked;
            pauseButton.clicked -= PauseButtonClicked;
            reloadButton.clicked -= ReloadButtonClicked;
            playContinuoslyButton.clicked -= PlayContinuouslyButtonClicked;
            nextTimePointButton.clicked -= NextTimePointButtonClicked;

            planPanelButton.clicked -= PlanPanelButtonClicked;
            actionTabButton.clicked -= ActionTabButtonClicked;
            objectInfoButton.clicked -= ObjectInfoButtonClicked;
            cameraControlsButton.clicked -= CameraControlsButtonClicked;
        }

        // Events
        private void BackButtonClicked()
        {
            throw new System.NotImplementedException("Back to Menu");
        }

        private void PlayButtonClicked()
        {
            Time.timeScale = 1;

            Controller.Instance.StartVisualisation();

            playButton.SetEnabled(false);
            pauseButton.SetEnabled(true);
        }

        private void PauseButtonClicked()
        {
            Time.timeScale = 0;
            playButton.SetEnabled(true);
            pauseButton.SetEnabled(false);
        }

        private void ReloadButtonClicked()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        private void NextTimePointButtonClicked()
        {
            Debug.LogWarning("Next Time Point");
            _controller.Advance();
        }

        private void PlayContinuouslyButtonClicked()
        {
            _controller.AdvanceContinuously();
            pauseButton.SetEnabled(true);
        }

        private void PlanPanelButtonClicked()
        {
            PlanListUI.ToggleVisibility();
        }

        private void ActionTabButtonClicked()
        {
            actionInfo.style.display = actionInfo.style.display == DisplayStyle.None ? DisplayStyle.Flex : DisplayStyle.None;
        }


        private void ObjectInfoButtonClicked()
        {
            StateListUI.ToggleVisibility();
        }

        private void CameraControlsButtonClicked()
        {
            cameraHints.style.display = cameraHints.style.display == DisplayStyle.None ? DisplayStyle.Flex : DisplayStyle.None;
            cameraHints.schedule.Execute(() => cameraHints.style.display = DisplayStyle.None).StartingIn(3000);
        }
    }
}