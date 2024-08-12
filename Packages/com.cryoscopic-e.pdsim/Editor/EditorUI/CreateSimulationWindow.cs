using System.Collections;
using PDSim.Connection;
using PDSim.Protobuf;
using PDSim.Utils;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.SceneTemplate;
using UnityEngine;
using UnityEngine.UIElements;

namespace PDSim.Editor.EditorUI
{
    public class CreateSimulationWindow : EditorWindow
    {
        [MenuItem("PDSim/Create Simulation")]
        public static void ShowWindow()
        {
            var wnd = GetWindow<CreateSimulationWindow>();
            wnd.titleContent = new GUIContent("CreateSimulationWindow");
        }
        private bool _connectionStatus = false;
        private Label _connectionStatusLabel;
        private TextField _simulationNameField;
        private Button _createSimulationButton;
        private Button _cancelButton;
        public void CreateGUI()
        {
            // Set Window not resizable
            this.minSize = new Vector2(365, 325);
            this.maxSize = this.minSize;

            // Each editor window contains a root VisualElement object
            var root = rootVisualElement;

            // Import UXML
            var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Packages/com.cryoscopic-e.pdsim/Resources/EditorUI/CreateSimulationWindow.uxml");
            var fromUxml = visualTree.Instantiate();
            root.Add(fromUxml);

            _connectionStatusLabel = root.Q<Label>("Status");
            _simulationNameField = root.Q<TextField>("SimulationName");
            _createSimulationButton = rootVisualElement.Q<Button>("CreateSimulationButton");
            _cancelButton = rootVisualElement.Q<Button>("CancelButton");

            SetButtonListeners();
            EditorCoroutineUtility.StartCoroutine(TestConnection(), this);
        }


        /// <summary>
        ///  Test connection to PDSim Backend Server
        ///
        ///  If connection is successful or unsuccessful, the status label will be updated accordingly .
        /// </summary>
        /// <returns></returns>
        private IEnumerator TestConnection()
        {
            ToggleButtons(false);

            _connectionStatusLabel.style.color = Color.yellow;
            _connectionStatusLabel.text = "Testing connection to backend...";

            var request = new BackendTestConnectionRequest();
            var response = request.Connect();
            _connectionStatusLabel.style.color = response["status"]?.ToString() switch
            {
                "OK" => Color.green,
                "TO" => Color.red,
                _ => Color.black
            };
            _connectionStatusLabel.text = response["status"]?.ToString() switch
            {
                "OK" => "Connected!",
                "TO" => "Disconnected!",
                _ => "Connection Error"
            };

            _connectionStatus = response["status"]?.ToString() == "OK";

            // Display error message if connection failed
            if (!_connectionStatus)
                EditorUtility.DisplayDialog("Connection Error", "Check server is running!", "OK");

            ToggleButtons(true);
            yield return null;
        }

        private IEnumerator ReadFromServer()
        {
            var request_problem = new ProtobufRequest("problem");
            var request_plan = new ProtobufRequest("plan");

            var response_problem = request_problem.Connect();

            var response_plan = request_plan.Connect();

            var reader = new ProtobufReader();


            // Create Simulation Scene
            var simulationName = _simulationNameField.value;
            AssetUtils.CreateFolders(simulationName);
            reader.Read(response_problem, response_plan, simulationName);
            yield return null;
        }

        private IEnumerator InitSimulation()
        {
            // Disable buttons
            ToggleButtons(false);

            // Check  connection
            yield return EditorCoroutineUtility.StartCoroutine(TestConnection(), this);

            // Check if connection is successful
            if (!_connectionStatus)
            {
                ToggleButtons(true);
                // Display error message
                EditorUtility.DisplayDialog("Error", "Connection to backend failed!", "OK");
                yield break;
            }

            // Display loading bar 
            EditorUtility.DisplayProgressBar("Collecting Protobuf Model", "Creating Simulation", 0.5f);

            // Launch Parsing
            yield return EditorCoroutineUtility.StartCoroutine(ReadFromServer(), this);

            EditorUtility.ClearProgressBar();

            // Create Simulation Scene
            CreateSimulationScene();

            yield return null;
        }

        private void CreateSimulationScene()
        {
            var sceneTemplate =
                AssetDatabase.LoadAssetAtPath<SceneTemplateAsset>(
                    "Packages/com.cryoscopic-e.pdsim/Runtime/Scenes/Templates/PdSimSimulationScene.scenetemplate");

            var newScenePath = AssetUtils.GetCurrentSimulationScenePath(_simulationNameField.value);

            var result = SceneTemplateService.Instantiate(sceneTemplate, false, newScenePath);
            EditorSceneManager.SaveScene(result.scene, newScenePath);
            Close();
        }

        /// <summary>
        ///  Toggle buttons on/off
        /// </summary>
        /// <param name="enabled"></param>
        private void ToggleButtons(bool enabled)
        {
            _createSimulationButton.SetEnabled(enabled);
            _cancelButton.SetEnabled(enabled);
        }

        /// <summary>
        ///   Validates the form. Returns true if the form is valid, false otherwise.
        ///  If the form is invalid, the validationMessage will contain the error message.
        /// </summary>
        /// <param name="validationMessage">
        ///  The validation message.
        /// </param>
        /// <returns>
        ///   <c>true</c> if the form is valid; otherwise, <c>false</c>.
        /// </returns>
        private bool ValidateForm(out string validationMessage)
        {
            // Check if simulation name is empty
            _simulationNameField.value = _simulationNameField.value.Trim();
            if (string.IsNullOrEmpty(_simulationNameField.value) || string.IsNullOrWhiteSpace(_simulationNameField.value))
            {
                validationMessage = "Simulation name cannot be empty or whitespace.";
                return false;
            }

            // Check Simulation Exists
            if (AssetUtils.SimulationExists(_simulationNameField.value))
            {
                validationMessage = "Simulation with that name already exists.";
                return false;
            }

            validationMessage = "";
            return true;
        }

        #region Buttons Clicked Callbacks

        /// <summary>
        ///  Sets the button listeners.
        ///  This function is called when the window is created.
        /// </summary>
        private void SetButtonListeners()
        {

            _createSimulationButton.clicked += CreateSimulationButtonClicked;

            _cancelButton.clicked += CancelButtonClicked;

            var refreshServerButton = rootVisualElement.Q<Button>("RefreshConnectionButton");
            refreshServerButton.clicked += RefreshConnectionButtonClicked;
        }

        /// <summary>
        ///  Called when the create simulation button is clicked.
        ///  Validates the form and creates the simulation.
        ///  If the form is invalid, the validation message will be displayed.
        ///  If the form is valid, the simulation will be created.
        /// </summary>
        private void CreateSimulationButtonClicked()
        {
            if (ValidateForm(out var validationMessage))
            {
                EditorCoroutineUtility.StartCoroutine(InitSimulation(), this);
            }
            else
            {
                EditorUtility.DisplayDialog("Error", validationMessage, "OK");
            }
        }

        /// <summary>
        ///  Cancel button clicked.
        /// </summary>
        private void CancelButtonClicked()
        {
            Close();
        }

        /// <summary>
        ///  Refresh connection button clicked.
        ///  Tests the connection to the backend server.
        /// </summary>
        private void RefreshConnectionButtonClicked()
        {
            EditorCoroutineUtility.StartCoroutine(TestConnection(), this);
        }
        #endregion
    }

}
