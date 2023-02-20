using UnityEditor;
using UnityEngine;
using System.IO;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace Editor
{
    internal class NewSimulationWindow : EditorWindow
    {
        public const string SimulationsBaseDir = "Assets/Simulations/";

        private string _simulationName;
        private TextAsset _domain;
        private TextAsset _problem;
        
        //[ MenuItem( "PDDL Simulation/New Simulation" ) ]
        public static void Init()
        {
            var window = (NewSimulationWindow)GetWindow(typeof(NewSimulationWindow));
            window.titleContent = new GUIContent("New Simulation");
            window.minSize = new Vector2(300,200);
            window.maxSize = new Vector2(300,200);
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
            
            GUILayout.BeginVertical();
            GUILayout.Label("Domain file:", EditorStyles.boldLabel);
            EditorGUI.BeginChangeCheck();
            var domain = (TextAsset) EditorGUILayout.ObjectField(_domain, typeof(TextAsset), false);
            if (EditorGUI.EndChangeCheck())
            {
                if(domain != null)
                    _domain = domain;
            }
            GUILayout.EndVertical();
            
            GUILayout.BeginVertical();
            GUILayout.Label("Problem file:", EditorStyles.boldLabel);
            EditorGUI.BeginChangeCheck();
            var problem = (TextAsset) EditorGUILayout.ObjectField(_problem, typeof(TextAsset), false);
            if (EditorGUI.EndChangeCheck())
            {
                if(problem != null)
                    _problem = problem;
            }
            GUILayout.EndVertical();
            
            GUILayout.Space(15);
            
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Create"))
            {
                if (_domain == null || _problem == null)
                {
                    EditorApplication.Beep();
                    EditorUtility.DisplayDialog("File Missing","Please Provide Domain And Problem files", "Close");
                }
                else
                {
                    CreateSimulation();
                    GUIUtility.ExitGUI();
                }
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
            var path = SimulationsBaseDir + _simulationName;
            
            // Check if simulation name exist
            if(Directory.Exists(path))
            {
                EditorUtility.DisplayDialog("Error", "Simulation " + _simulationName + " already Exist!", "Cancel");
                return;
            }
            // Create simulation base folder
            Directory.CreateDirectory(path);

            // Create Simulation Environment Prefab
            var newSimEnv = CreateInstance<SimulationEnvironment> ();
            newSimEnv.problem = _problem;
            var newSimEnvPath = AssetDatabase.GenerateUniqueAssetPath (path + "/" + _simulationName + " Environment.asset");
            AssetDatabase.CreateAsset (newSimEnv, newSimEnvPath);
            
            // Create Folder for Types Models
            Directory.CreateDirectory(path+"/Types Models");
            
            // Create Folder for Predicates Behaviours
            Directory.CreateDirectory(path+"/Predicates Behaviours");
            
            
            var newScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            newScene.name = _simulationName;
            var simulationManagerAsset = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/SimulationManager.prefab");
            var sceneElementsAsset = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Scene Elements.prefab");
            Instantiate(sceneElementsAsset);
            var simulationManager = Instantiate(simulationManagerAsset);
            // Initialize simulation manager
            simulationManager.GetComponent<SimulationManager>().domain = _domain;
            simulationManager.GetComponent<SimulationManager>().simulationName = _simulationName;
            simulationManager.GetComponent<SimulationManager>().simulationEnvironment = newSimEnv;
            
            // Save changes
            EditorSceneManager.SaveScene(newScene, "Assets/Scenes/"+_simulationName+".unity");
            AssetDatabase.SaveAssets ();
            AssetDatabase.Refresh();
            Lightmapping.BakeAsync();
            Close();
        }
    }
}