using UnityEngine.SceneManagement;
using UnityEngine.Windows;

namespace PDSim.Utils
{
    public static class AssetUtils
    {
        private const string SimulationsRootFolder = "Assets/_PDSim_Simulations/";
        private const string SimObjectsFolder = "Objects/";
        private const string SimProblemFolder = "Problems/";

        private static void CreateFolderIfNotExist(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);

            }
        }

        public static void CreateFolders(string sceneName)
        {
            var simulationPath = SimulationsRootFolder + sceneName + "/";
            CreateFolderIfNotExist(simulationPath + SimObjectsFolder);
            CreateFolderIfNotExist(simulationPath + SimProblemFolder);
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
        //
        // public static string GetProblemResource(string problemName)
        // {
        //     return SimulationsRootFolder + SceneManager.GetActiveScene().name + "/" + SimProblemFolder + problemName + ".asset";
        // }
        //
        // public static string GetCurrentSimulationPath(string sceneName)
        // {
        //     var simulationPath = SimulationsRootFolder + sceneName + "/";
        //     return simulationPath;
        // }



        public static string GetCurrentSimulationScenePath(string sceneName)
        {
            var simulationPath = SimulationsRootFolder + sceneName + "/";

            CreateFolderIfNotExist(simulationPath);

            return simulationPath + sceneName + ".unity";
        }

        public static bool SimulationSceneExists(string simulationName)
        {
            var simulationPath = SimulationsRootFolder + simulationName + "/" + simulationName + ".unity";

            return File.Exists(simulationPath);
        }
    }
}