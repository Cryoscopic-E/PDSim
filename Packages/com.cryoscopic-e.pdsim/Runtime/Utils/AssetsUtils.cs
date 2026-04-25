using UnityEditor;
using UnityEngine.Windows;

namespace PDSim.Utils
{
#if UNITY_EDITOR
    /// <summary>
    /// Utility class for managing assets and folders in the Unity project.
    /// </summary>
    public static class AssetUtils
    {
        #region Private Internals

        private const string _simData = "Data/";
        private const string _simObjectsFolder = "Objects/";
        private const string _simScriptsFolder = "Scripts/";

        /// <summary>
        /// Creates a folder if it does not already exist.
        /// </summary>
        /// <param name="path">The path to the folder.</param>
        private static void CreateFolderIfDontExist(string path)
        {
            if (!DirectoryExists(path))
            {
                Directory.CreateDirectory(path);
            }
        }

        #endregion

        #region Public API

        /// <summary>
        /// Check if a file exists in the project.
        /// </summary>
        /// <param name="path">The path to the file.</param>
        /// <returns>True if the file exists, false otherwise.</returns>
        public static bool FileExists(string path)
        {
            return File.Exists(path);
        }

        /// <summary>
        /// Check if a folder exists in the project.
        /// </summary>
        /// <param name="path">The path to the folder.</param>
        /// <returns>True if the folder exists, false otherwise.</returns>
        public static bool DirectoryExists(string path)
        {
            return Directory.Exists(path);
        }

        /// <summary>
        /// Create all the folders needed to store a simulation in the project.
        /// </summary>
        /// <param name="sceneName">The name of the scene.</param>
        public static void CreateFolders(string sceneName)
        {
            // Create the root folder for all simulations
            CreateFolderIfDontExist(CommonPaths.SimulationsRootFolder);
            // Create the folder for the current simulation
            var simulationPath = CommonPaths.SimulationsRootFolder + sceneName + "/";
            CreateFolderIfDontExist(simulationPath);
            // Folder for the objects
            CreateFolderIfDontExist(simulationPath + _simObjectsFolder);
            // Folder for data
            CreateFolderIfDontExist(simulationPath + _simData);
            // Folder for scripts
            CreateFolderIfDontExist(simulationPath + _simScriptsFolder);
        }

        /// <summary>
        /// Gets the path to the simulation data folder.
        /// </summary>
        /// <param name="sceneName">The name of the scene.</param>
        /// <returns>The path to the simulation data folder.</returns>
        public static string GetSimulationDataPath(string sceneName)
        {
            var simulationPath = CommonPaths.SimulationsRootFolder + sceneName + "/" + _simData;
            return simulationPath;
        }

        /// <summary>
        /// Gets the path to the simulation objects folder.
        /// </summary>
        /// <param name="sceneName">The name of the scene.</param>
        /// <returns>The path to the simulation objects folder.</returns>
        public static string GetSimulationObjectsPath(string sceneName)
        {
            var simulationPath = CommonPaths.SimulationsRootFolder + sceneName + "/" + _simObjectsFolder;
            return simulationPath;
        }

        /// <summary>
        /// Gets the path to the simulation scripts folder.
        /// </summary>
        /// <param name="sceneName">The name of the scene.</param>
        /// <returns>The path to the simulation scripts folder.</returns>
        public static string GetSimulationScriptsPath(string sceneName)
        {
            var simulationPath = CommonPaths.SimulationsRootFolder + sceneName + "/" + _simScriptsFolder;
            return simulationPath;
        }

        /// <summary>
        /// Gets the path to the simulation behaviors folder.
        /// </summary>
        /// <param name="sceneName">The name of the scene.</param>
        /// <returns>The path to the simulation behaviors folder.</returns>
        public static string GetSimulationBehaviorsPath(string sceneName)
        {
            return GetSimulationScriptsPath(sceneName) + "/Behaviors";
        }

        /// <summary>
        /// Gets the path to the simulation animations folder.
        /// </summary>
        /// <param name="sceneName">The name of the scene.</param>
        /// <returns>The path to the simulation animations folder.</returns>
        public static string GetSimulationAnimationsPath(string sceneName)
        {
            return GetSimulationScriptsPath(sceneName) + "/Animations";
        }

        /// <summary>
        /// Loads an asset at the specified path.
        /// </summary>
        /// <typeparam name="T">The type of the asset.</typeparam>
        /// <param name="path">The path to the asset.</param>
        /// <returns>The loaded asset, or null if not found.</returns>
        public static T GetAsset<T>(string path) where T : UnityEngine.Object
        {
            return AssetDatabase.LoadAssetAtPath<T>(path);
        }

        /// <summary>
        /// Creates a scene path for the specified scene name.
        /// </summary>
        /// <param name="sceneName">The name of the scene.</param>
        /// <returns>The full path to the scene file.</returns>
        public static string CreateScenePath(string sceneName)
        {
            var simulationPath = CommonPaths.SimulationsRootFolder + sceneName + "/";

            CreateFolderIfDontExist(simulationPath);

            return simulationPath + sceneName + ".unity";
        }

        /// <summary>
        /// Checks if a simulation with the specified name exists.
        /// </summary>
        /// <param name="simulationName">The name of the simulation.</param>
        /// <returns>True if the simulation exists, false otherwise.</returns>
        public static bool SimulationExists(string simulationName)
        {
            var simulationPath = CommonPaths.SimulationsRootFolder + simulationName + "/";

            return DirectoryExists(simulationPath);
        }

        #endregion
    }
#endif
}
