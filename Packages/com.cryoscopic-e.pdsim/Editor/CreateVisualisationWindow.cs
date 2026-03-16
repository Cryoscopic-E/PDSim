using PDSim.ScriptableObjects;
using PDSim.Utils;
using PDSimAPI;
using System.Collections;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.SceneTemplate;
using UnityEngine;
using UnityEngine.UIElements;
namespace PDSim.Editor
{
    public class CreateVisualisationWindow : EditorWindow
    {
        [MenuItem("PDSim/Create Visualisation")]
        public static void ShowWindow()
        {
            var wnd = GetWindow<CreateVisualisationWindow>();
            wnd.titleContent = new GUIContent("Create Visualisation");
        }
        private bool _connectionStatus = false;
        private Label _connectionStatusLabel;
        private TextField _visualisationNameField;
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
            var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(CommonPaths.CREATESIM_WINDOW_UI);
            var fromUxml = visualTree.Instantiate();
            root.Add(fromUxml);

            _connectionStatusLabel = root.Q<Label>("Status");
            _visualisationNameField = root.Q<TextField>("SimulationName");
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



            // Create Simulation Scene
            var simulationName = _visualisationNameField.value;
            AssetUtils.CreateFolders(simulationName);

            yield return SaveProto(response_problem, response_plan, simulationName);
        }

        private IEnumerator SaveProto(byte[] problem, byte[] plan, string name)
        {

            //Save asset
            var simulationDataRoot = AssetUtils.GetSimulationDataPath(name);
            var problemPath = simulationDataRoot + "Problem.asset";
            var planGenPath = simulationDataRoot + "Plan.asset";

            var planningProblem = CreateInstance<PlanningProblem>();
            planningProblem.proto = problem;
            EditorUtility.SetDirty(planningProblem);

            var planGen = CreateInstance<PlanGeneration>();
            planGen.proto = plan;
            EditorUtility.SetDirty(planGen);

            AssetDatabase.CreateAsset(planningProblem, problemPath);
            AssetDatabase.CreateAsset(planGen, planGenPath);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            yield return null;
        }

        private IEnumerator InitVisualisation()
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
            var sceneTemplate = AssetDatabase.LoadAssetAtPath<SceneTemplateAsset>(CommonPaths.TEMPLATE_VISUALISATION_SCENE);

            var newScenePath = AssetUtils.CreateScenePath(_visualisationNameField.value);
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
            _visualisationNameField.value = _visualisationNameField.value.Trim();
            if (string.IsNullOrEmpty(_visualisationNameField.value) || string.IsNullOrWhiteSpace(_visualisationNameField.value))
            {
                validationMessage = "Simulation name cannot be empty or whitespace.";
                return false;
            }

            // Check Simulation Exists
            if (AssetUtils.SimulationExists(_visualisationNameField.value))
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
                EditorCoroutineUtility.StartCoroutine(InitVisualisation(), this);
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
