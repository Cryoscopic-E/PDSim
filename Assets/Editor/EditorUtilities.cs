using UnityEditor;
using UnityEngine;
using System.IO;
namespace Editor
{
    internal class NewSimulationWindow : EditorWindow
    {
        public static readonly string SIMULATIONS_BASE_DIR = "Assets/Simulations/";
        
        private string _simulationName;
        
        [ MenuItem( "PDDL Simulation/New Simulation" ) ]
        public static void Init()
        {
            var window = (NewSimulationWindow)GetWindow(typeof(NewSimulationWindow));
            window.titleContent = new GUIContent("New Simulation");
            window.minSize = new Vector2(300,100);
            window.maxSize = new Vector2(300,100);
            window.Show();
            window.Focus();
        }

        void OnGUI()
        {
            GUILayout.BeginVertical();
            GUILayout.Label("Simulation name:", EditorStyles.boldLabel);
            GUILayout.Space(10);
            _simulationName = EditorGUILayout.TextField(_simulationName);
            GUILayout.EndVertical();
            GUILayout.Space(15);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Create"))
            {
                CreateSimulation();
                GUIUtility.ExitGUI();
            }

            if (GUILayout.Button("Cancel"))
            {
                Close();
            }
            GUILayout.EndHorizontal();
        }

        private void CreateSimulation()
        {
            if (string.IsNullOrEmpty(_simulationName))
            {
                EditorUtility.DisplayDialog("Error", "Insert a valid name for the new simulation", "Cancel");
                return;
            }
            
            _simulationName = _simulationName.Trim();
            var path = SIMULATIONS_BASE_DIR + _simulationName;
            
            // Check if simulation name exist
            if(Directory.Exists(path))
            {
                EditorUtility.DisplayDialog("Error", "Simulation " + _simulationName + " already Exist!", "Cancel");
                return;
            }
            // Create simulation base folder
            Directory.CreateDirectory(path);
            
            // Create Simulation Setting Prefab
            var newSimSetting = CreateInstance<SimulationSettings> ();
            var newSimSettingsPath = AssetDatabase.GenerateUniqueAssetPath (path + "/" + _simulationName + " Settings.asset");
            AssetDatabase.CreateAsset (newSimSetting, newSimSettingsPath);
            
            // Create Simulation Environment Prefab
            var newSimEnv = CreateInstance<SimulationEnvironment> ();
            newSimEnv.simulationName = _simulationName;
            var newSimEnvPath = AssetDatabase.GenerateUniqueAssetPath (path + "/" + _simulationName + " Environment.asset");
            AssetDatabase.CreateAsset (newSimEnv, newSimEnvPath);
            
            // Create Folder for Types Models
            Directory.CreateDirectory(path+"/Types Models");
            
            // Create Folder for Predicates Behaviours
            Directory.CreateDirectory(path+"/Predicates Behaviours");
            
            //TODO Create Scene
            //TODO Instantiate Scene Manager in the new scene
            //TODO Instantiate Scene Elements in the new scene
            
            // Save changes
            AssetDatabase.SaveAssets ();
            AssetDatabase.Refresh();
            Close();
        }
    }
}