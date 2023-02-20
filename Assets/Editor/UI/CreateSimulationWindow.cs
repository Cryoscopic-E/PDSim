using System.Collections;
using Newtonsoft.Json.Linq;
using PDSim.Utils;
using PDSim.Connection;
using PDSim.Simulation;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Editor.UI
{
    public class CreateSimulationWindow : EditorWindow
    {
        [MenuItem("PDSim/CreateSimulationWindow")]
        public static void ShowWindow()
        {
            var wnd = GetWindow<CreateSimulationWindow>();
            wnd.titleContent = new GUIContent("CreateSimulationWindow");
        }
        private  bool _connectionStatus = false;
        private Label _connectionStatusLabel;
        private TextField _simulationNameField;
        private TextField _domainPathText;
        private TextField _problemPathText;
        private Button _createSimulationButton;
        private Button _cancelButton;
        private JObject _parsedJson;
        private  ServerStatus _serverStatus;
        public void CreateGUI()
        {
            // Set Window not resizable
            this.minSize = new Vector2(365, 325);
            this.maxSize = this.minSize;
            
            // Each editor window contains a root VisualElement object
            var root = rootVisualElement;
            
            // Import UXML
            var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/UI Toolkit/CreateSimulationWindow.uxml"); // TODO: Change path to relative
            var fromUxml = visualTree.Instantiate();
            root.Add(fromUxml);
            
            _connectionStatusLabel = root.Q<Label>("Status");
            _simulationNameField = root.Q<TextField>("SimulationName");
            _domainPathText = root.Q<TextField>("DomainPath");
            _problemPathText = root.Q<TextField>("ProblemPath");
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
            if(!_connectionStatus)
                EditorUtility.DisplayDialog("Connection Error", "Check server is running!", "OK");
            
            ToggleButtons(true);
            yield return null;
        }

        /// <summary>
        ///  Send Parse request to PDSim Backend Server
        /// </summary>
        /// <returns></returns>

        private IEnumerator ParseFiles()
        {
            var request = new BackendParseRequest(_domainPathText.value, _problemPathText.value);
            
            // Check  for errors (parse_error, syntax_error, assertion_error,error)
            var response = request.Connect();
            if (response["status"]?.ToString() == "OK")
            {
                Debug.Log("Parse Successful");
            }
            else
            {
                if (response["parse_error"] != null)
                {
                    EditorUtility.DisplayDialog("Parse Error", response["parse_error"].ToString(), "OK");
                    yield break;
                }
                if (response["syntax_error"] != null)
                {
                    EditorUtility.DisplayDialog("Syntax Error", response["syntax_error"].ToString(), "OK");
                    yield break;
                }
                if (response["assertion_error"] != null)
                {
                    EditorUtility.DisplayDialog("Assertion Error", response["assertion_error"].ToString(), "OK");
                    yield break;
                }
                if (response["error"] != null)
                {
                    EditorUtility.DisplayDialog("Error", response["error"].ToString(), "OK");
                    yield break;
                }
                
            }
            _parsedJson = response;
            yield return null;
        }

        private IEnumerator InitSimulation()
        {
            // Disable buttons
            ToggleButtons(false);
            
            // Check  connection
            yield return  EditorCoroutineUtility.StartCoroutine(TestConnection(), this);
                
            // Check if connection is successful
            if (!_connectionStatus)
            {
                ToggleButtons(true);
                // Display error message
                EditorUtility.DisplayDialog("Error", "Connection to backend failed!", "OK");
                yield break;
            }
            
            // Display loading bar 
            EditorUtility.DisplayProgressBar("Parsing Files", "Creating Simulation", 0.5f);
            
            // Launch Parsing
            yield return  EditorCoroutineUtility.StartCoroutine(ParseFiles(), this);
            
            EditorUtility.ClearProgressBar();
            
            // Create scene from template
            
            // Create PdSim Environment Scriptable Object

            if (_parsedJson != null && (_parsedJson!= null || !_parsedJson.ContainsKey("error")))
            {
                var instance = CreateInstance<PdSimEnvironment>();
                instance.name = _simulationNameField.value;
                instance.CreateInstance("dew", "fwe", _parsedJson);
                EditorUtility.SetDirty(instance);
                AssetDatabase.CreateAsset(instance, "Assets/" + _simulationNameField.value + ".asset");
            }
            

            // Save json to simulation folder
            
            // Close window
            Close();
            
            yield return null;
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

            // Check if domain path is empty
            if (string.IsNullOrEmpty(_domainPathText.text))
            {
                validationMessage = "Domain path cannot be empty";
                return false;
            }
            
            // Check if problem path is empty
            if (string.IsNullOrEmpty(_problemPathText.text))
            {
                validationMessage = "Problem path cannot be empty";
                return false;
            }
            
            // Domain Path is a directory
            if (AssetUtils.DirectoryExist(_domainPathText.text))
            {
                validationMessage = "Domain path cannot be a directory";
                return false;
            }
            
            // Problem Path is a directory
            if (AssetUtils.DirectoryExist(_problemPathText.text))
            {
                validationMessage = "Problem path cannot be a directory";
                return false;
            }
            
            // Check if domain file exists
            if (!AssetUtils.FileExists(_domainPathText.text))
            {
                validationMessage= "Domain file does not exist";
                return false;
            }
            
            // Check if problem file exists
            if (!AssetUtils.FileExists(_problemPathText.text))
            {
                validationMessage = "Problem file does not exist";
                return false;
            }
            
            validationMessage = "";
            return  true;
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

            var domainButton = rootVisualElement.Q<Button>("DomainFileSearchButton");
            domainButton.clicked += DomainFileSelectionButtonClicked;

            var problemButton = rootVisualElement.Q<Button>("ProblemFileSearchButton");
            problemButton.clicked += ProblemFileSelectionButtonClicked;

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
        ///  Domain file selection button clicked.
        ///  Opens a file selection dialog to select a domain file.
        /// </summary>
        private void DomainFileSelectionButtonClicked()
        {
            _domainPathText.value = EditorUtility.OpenFilePanel("Select Domain", "", "pddl");
        }
        
        /// <summary>
        ///  Problem file selection button clicked.
        ///  Opens a file selection dialog to select a problem file.
        /// </summary>
        private void ProblemFileSelectionButtonClicked()
        {
            _problemPathText.value = EditorUtility.OpenFilePanel("Select Problem", "", "pddl");
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

    internal struct ServerStatus
    {
        public  bool IsConnected { get; set; }
        public bool ParseRequested { get; set; }
        public bool PlanRequested { get; set; }
        public bool ParseError { get; set; }
        public bool PlanError { get; set; }
        public string ParseErrorMessage { get; set; }
        public string PlanErrorMessage { get; set; }
    }
}
