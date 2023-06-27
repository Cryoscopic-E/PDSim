using PDSim.Components;
using PDSim.Simulation;
using PDSim.UI;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class MainUI : MonoBehaviour
{

    [SerializeField]
    PlanListUI PlanListUI;

    [SerializeField]
    StateListUI StateListUI;

    Button backButton;
    Button playButton;
    Button pauseButton;
    Button prevButton;
    Button nextButton;

    Button planPanelButton;
    Button actionTabButton;
    Button simulationControlsButton;
    Button objectInfoButton;
    Button cameraControlsButton;

    Label actionStatus;
    Label predicateAnimated;

    VisualElement actionInfo;

    private PdSimManager simManager;

    private void Awake()
    {
        simManager = PdSimManager.Instance;

        var root = GetComponent<UIDocument>().rootVisualElement;

        backButton = root.Q<Button>("BackButton");
        playButton = root.Q<Button>("PlayButton");

        pauseButton = root.Q<Button>("PauseButton");
        pauseButton.SetEnabled(false);

        prevButton = root.Q<Button>("PrevButton");
        prevButton.SetEnabled(false);

        nextButton = root.Q<Button>("NextButton");
        nextButton.SetEnabled(false);

        planPanelButton = root.Q<Button>("PlanPanelButton");
        actionTabButton = root.Q<Button>("ActionTabButton");
        objectInfoButton = root.Q<Button>("ObjectInfoButton");
        cameraControlsButton = root.Q<Button>("CameraControlsButton");

        actionInfo = root.Q<VisualElement>("ActionInfo");


        backButton.clicked += BackButtonClicked;

        playButton.clicked += PlayButtonClicked;

        pauseButton.clicked += PauseButtonClicked;

        prevButton.clicked += PrevButtonClicked;
        nextButton.clicked += NextButtonClicked;

        planPanelButton.clicked += PlanPanelButtonClicked;
        actionTabButton.clicked += ActionTabButtonClicked;
        objectInfoButton.clicked += ObjectInfoButtonClicked;
        cameraControlsButton.clicked += CameraControlsButtonClicked;



        var simulationNameLabel = root.Q<Label>("SimulationName");
        var sceneName = SceneManager.GetActiveScene().name;
        simulationNameLabel.text = char.ToUpper(sceneName[0]) + sceneName.Substring(1);


        actionStatus = root.Q<Label>("Action");
        predicateAnimated = root.Q<Label>("Predicate");


        // Simulation Manager Events
        simManager.OnSimulationReady += SimulationReady;
        simManager.OnSimulationInitBlock += SimulationInitBlock;
        simManager.OnSimulationActionBlock += SimulationActionBlock;
        simManager.OnSimulationStep += SimulationStep;
        simManager.OnSimulationFinished += SimulationFinished;
        simManager.OnSimulationObjectHovered += SimulationObjectHovered;
        simManager.OnSimulationObjectUnhovered += SimulationObjectUnhovered;

    }

    private void SimulationReady(List<PdPlanAction> planList)
    {
        actionStatus.text = "Ready";
        predicateAnimated.text = "";
        PlanListUI.InitializePlanList(planList);
    }

    private void SimulationActionBlock(string actionName, int index)
    {
        actionStatus.text = actionName;
        PlanListUI.HighlightCurrentAction(index);
    }

    private void SimulationInitBlock()
    {
        actionStatus.text = "Init Block";
    }

    private void SimulationStep(string fluent)
    {
        predicateAnimated.text = fluent;
    }

    private void SimulationFinished()
    {
        playButton.SetEnabled(false);
        pauseButton.SetEnabled(false);
        actionStatus.text = "Simulation Finished";
        predicateAnimated.text = "";
    }

    private void SimulationObjectHovered(string objectName, List<PdBooleanPredicate> state)
    {

        StateListUI.InitializePlanList(objectName, state);
    }
    private void SimulationObjectUnhovered()
    {
        StateListUI.Clear();
    }


    private void OnDisable()
    {
        backButton.clicked -= BackButtonClicked;
        playButton.clicked -= PlayButtonClicked;
        pauseButton.clicked -= PauseButtonClicked;
        prevButton.clicked -= PrevButtonClicked;
        nextButton.clicked -= NextButtonClicked;

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
        if (simManager.SimulationRunning)
        {
            // Resume the simulation
            Time.timeScale = 1;

        }
        else
        {
            simManager.StartSimulation();
        }


        playButton.SetEnabled(false);
        pauseButton.SetEnabled(true);
    }

    private void PauseButtonClicked()
    {
        Time.timeScale = 0;
        playButton.SetEnabled(true);
        pauseButton.SetEnabled(false);
    }

    private void PrevButtonClicked()
    {
        throw new System.NotImplementedException("Prev Animation");
    }

    private void NextButtonClicked()
    {
        throw new System.NotImplementedException("Next Animation");
    }

    private void PlanPanelButtonClicked()
    {
        PlanListUI.ToggleVisibility();
    }

    private void ActionTabButtonClicked()
    {
        actionInfo.visible = !actionInfo.visible;
    }


    private void ObjectInfoButtonClicked()
    {
        Debug.Log("Object Info Button clicked");
    }

    private void CameraControlsButtonClicked()
    {
        Debug.Log("Camera Controls Button clicked");
    }


}
