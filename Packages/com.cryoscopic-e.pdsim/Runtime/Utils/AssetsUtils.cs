using UnityEditor;
using UnityEngine.Windows;

namespace PDSim.Utils
{
    # if UNITY_EDITOR
    public static class AssetUtils
    {
        private const string SimData = "Data/";
        private const string SimObjectsFolder = "Objects/";

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
        public static bool FileExists(string path)
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
        public static bool DirectoryExist(string path)
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
            CreateFolderIfDontExist(CommonPaths.SIMULATIONS_ROOT_FOLDER);
            // Create the folder for the current simulation
            var simulationPath = CommonPaths.SIMULATIONS_ROOT_FOLDER + sceneName + "/";
            CreateFolderIfDontExist(simulationPath);
            // Folder for the objects
            CreateFolderIfDontExist(simulationPath + SimObjectsFolder);
            // Folder for data
            CreateFolderIfDontExist(simulationPath + SimData);
        }

        public static string GetSimulationDataPath(string sceneName)
        {
            var simulationPath = CommonPaths.SIMULATIONS_ROOT_FOLDER + sceneName + "/" + SimData;
            return simulationPath;
        }

        public static string GetSimulationObjectsPath(string sceneName)
        {
            var simulationPath = CommonPaths.SIMULATIONS_ROOT_FOLDER + sceneName + "/" + SimObjectsFolder;
            return simulationPath;
        }

        public static T GetAsset<T>(string path) where T : UnityEngine.Object
        {
            return AssetDatabase.LoadAssetAtPath<T>(path);
        }


        public static string GetCurrentSimulationScenePath(string sceneName)
        {
            var simulationPath = CommonPaths.SIMULATIONS_ROOT_FOLDER + sceneName + "/";

            CreateFolderIfDontExist(simulationPath);

            return simulationPath + sceneName + ".unity";
        }

        public static bool SimulationExists(string simulationName)
        {
            var simulationPath = CommonPaths.SIMULATIONS_ROOT_FOLDER + simulationName + "/";

            return DirectoryExist(simulationPath);
        }

        //public static void CreateSimulationScene(string simulationName)
        //{
        //    var sceneTemplate = AssetDatabase.LoadAssetAtPath<SceneTemplateAsset>(SceneTemplatePath);

        //    var newScenePath = GetCurrentSimulationScenePath(simulationName);

        //    CreateFolders(simulationName);

        //    var result = SceneTemplateService.Instantiate(sceneTemplate, false, newScenePath);

        //    EditorSceneManager.SaveScene(result.scene, newScenePath);
        //}
    }
    #endif
}