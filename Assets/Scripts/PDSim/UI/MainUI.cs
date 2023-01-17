using UnityEngine;
using UnityEngine.UIElements;

public class MainUI : MonoBehaviour
{

    [SerializeField]
    PlanListUI PlanListUI;

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

    private void OnEnable()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;

        backButton = root.Q<Button>("BackButton");
        playButton = root.Q<Button>("PlayButton");
        pauseButton = root.Q<Button>("PauseButton");
        prevButton = root.Q<Button>("PrevButton");
        nextButton = root.Q<Button>("NextButton");

        planPanelButton = root.Q<Button>("PlanPanelButton");
        actionTabButton = root.Q<Button>("ActionTabButton");
        simulationControlsButton = root.Q<Button>("SimulationControlsButton");
        objectInfoButton = root.Q<Button>("ObjectInfoButton");
        cameraControlsButton = root.Q<Button>("CameraControlsButton");


        backButton.clicked += BackButtonClicked;
        playButton.clicked += PlayButtonClicked;
        pauseButton.clicked += PauseButtonClicked;
        prevButton.clicked += PrevButtonClicked;
        nextButton.clicked += NextButtonClicked;

        planPanelButton.clicked += PlanPanelButtonClicked;
        actionTabButton.clicked += ActionTabButtonClicked;
        simulationControlsButton.clicked += SimulationControlsButtonClicked;
        objectInfoButton.clicked += ObjectInfoButtonClicked;
        cameraControlsButton.clicked += CameraControlsButtonClicked;

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
        simulationControlsButton.clicked -= SimulationControlsButtonClicked;
        objectInfoButton.clicked -= ObjectInfoButtonClicked;
        cameraControlsButton.clicked -= CameraControlsButtonClicked;
    }

    // Events
    private void BackButtonClicked()
    {
        Debug.Log("Back Button clicked");
    }

    private void PlayButtonClicked()
    {
        Debug.Log("Play Button clicked");
    }

    private void PauseButtonClicked()
    {
        Debug.Log("Pause Button clicked");
    }

    private void PrevButtonClicked()
    {
        Debug.Log("Prev Button clicked");
    }

    private void NextButtonClicked()
    {
        Debug.Log("Next Button clicked");
    }

    private void PlanPanelButtonClicked()
    {
        PlanListUI.ToggleVisibility();
    }

    private void ActionTabButtonClicked()
    {
        Debug.Log("Action Tab Button clicked");
    }

    private void SimulationControlsButtonClicked()
    {
        Debug.Log("Simulation Controls Button clicked");
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
