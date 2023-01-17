using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.SceneTemplate;
using UnityEngine.SceneManagement;
using UnityEngine.Windows;

namespace PDSim.Utils
{
    public static class AssetUtils
    {
        private const string SimulationsRootFolder = "Assets/_PDSim_Simulations/";
        private const string SimObjectsFolder = "Objects/";
        private const string SimProblemFolder = "Problems/";
        private const string SceneTemplatePath = "Assets/Scenes/Templates/PDSimSceneTemplate.scenetemplate";

        private static void CreateFolderIfDontExist(string path)
        {
            if (!DirectoryExist(path))
            {
                Directory.CreateDirectory(path);
            }
        }
        
        /// <summary>
        ///  Check if a file exists in the project
        /// </summary>
        /// <param name="path">
        /// The path to the file
        /// </param>
        /// <returns>
        /// True if the file exists, false otherwise
        /// </returns>
        public  static  bool FileExists(string path)
        {
            return File.Exists(path);
        }
        
        /// <summary>
        ///  Check if a folder exists in the project
        /// </summary>
        /// <param name="path">
        /// The path to the folder
        /// </param>
        /// <returns>
        /// True if the folder exists, false otherwise
        /// </returns>
        public  static  bool DirectoryExist(string path)
        {
            return Directory.Exists(path);
        }
        
        /// <summary>
        ///  Create all the folders needed to store a simulation in the project
        /// </summary>
        /// <param name="sceneName">
        /// The name of the scene
        /// </param>
        public static void CreateFolders(string sceneName)
        {
            // Create the root folder for all simulations
            CreateFolderIfDontExist(SimulationsRootFolder);
            // Create the folder for the current simulation
            var simulationPath = SimulationsRootFolder + sceneName + "/";
            // Folder for the objects
            CreateFolderIfDontExist(simulationPath + SimObjectsFolder);
            // Folder for different problems
            CreateFolderIfDontExist(simulationPath + SimProblemFolder);
        }

        public static string GetCurrentObjectsFolder()
        {
            var assetPath = SimulationsRootFolder + SceneManager.GetActiveScene().name + "/" + SimObjectsFolder;
            return assetPath;
        }

        public static string GenerateProblemAssetPath(string problemName)
        {
            var assetPath = SimulationsRootFolder + SceneManager.GetActiveScene().name + "/" + SimProblemFolder + problemName + ".asset";
            return assetPath;
        }
        public static string GetProblemResource(string problemName)
        {
            return SimulationsRootFolder + SceneManager.GetActiveScene().name + "/" + SimProblemFolder + problemName + ".asset";
        }
        
        public static string GetCurrentSimulationDirectoryRoot(string sceneName)
        {
            var simulationPath = SimulationsRootFolder + sceneName + "/";
            return simulationPath;
        }



        public static string GetCurrentSimulationScenePath(string sceneName)
        {
            var simulationPath = SimulationsRootFolder + sceneName + "/";

            CreateFolderIfDontExist(simulationPath);

            return simulationPath + sceneName + ".unity";
        }

        public static bool SimulationExists(string simulationName)
        {
            var simulationPath = SimulationsRootFolder + simulationName + "/";

            return DirectoryExist(simulationPath);
        }

        public  static  void  CreateSimulationScene(string simulationName)
        {
            var sceneTemplate = AssetDatabase.LoadAssetAtPath<SceneTemplateAsset>(SceneTemplatePath);

            var newScenePath =  GetCurrentSimulationScenePath(simulationName);
            
            CreateFolders(simulationName);
            
            var result = SceneTemplateService.Instantiate(sceneTemplate, false, newScenePath);
            
            EditorSceneManager.SaveScene(result.scene, newScenePath);
        }
    }
}