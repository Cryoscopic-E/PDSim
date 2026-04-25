using PDSim.Components;
using PDSim.Helpers;
using PDSim.ScriptableObjects;
using PDSim.Utils;
using Proto;
using UnityEditor;
using UnityEditor.SceneTemplate;
using UnityEngine.SceneManagement;

namespace PDSim.SceneTemplates
{
    /// <summary>
    /// Pipeline for the Visualisation Scene template, handles data connections after scene instantiation.
    /// </summary>
    public class VisualisationScenePipeline : SceneTemplatePipelineAdapter
    {
        #region Public Methods
        /// <summary>
        /// Validates if this template can be instantiated.
        /// </summary>
        /// <param name="sceneTemplateAsset">The scene template asset.</param>
        /// <returns>Always false to prevent manual instantiation from Unity menu.</returns>
        public override bool IsValidTemplateForInstantiation(SceneTemplateAsset sceneTemplateAsset)
        {
            // Don't want user create this scene using "New Scene" in Unity
            return false;
        }

        /// <summary>
        /// Called after the scene has been instantiated from the template.
        /// </summary>
        /// <param name="sceneTemplateAsset">The scene template asset.</param>
        /// <param name="scene">The new scene.</param>
        /// <param name="isAdditive">Whether the scene was added additively.</param>
        /// <param name="sceneName">The name of the scene.</param>
        public override void AfterTemplateInstantiation(SceneTemplateAsset sceneTemplateAsset, Scene scene, bool isAdditive, string sceneName)
        {
            // Connect data assets to simulation manager
            var simulationManager = Controller.Instance;
            var simulationDataRoot = AssetUtils.GetSimulationDataPath(scene.name);
            simulationManager.Problem = AssetUtils.GetAsset<ParsedProblem>(simulationDataRoot + "/Problem.asset");
            simulationManager.PlanGeneration = AssetUtils.GetAsset<PlanGeneration>(simulationDataRoot + "/Plan.asset");


            EditorUtility.SetDirty(simulationManager);

            var problemParser = Problem.Parser.ParseFrom(simulationManager.Problem.Proto);
            var planParser = PlanGenerationResult.Parser.ParseFrom(simulationManager.PlanGeneration.Proto);


            // Types
            var types = ProtobufReader.ReadTypes(problemParser);
            TypeHierarchy.Instance.Populate(types);

            // Fluents
            var fluents = ProtobufReader.ReadFluents(problemParser);
            
            Animations.Instance.InitialiseComponent(fluents);

            // Objects
            var problemObjects = ProtobufReader.ReadObjects(problemParser);

            ProblemObjects.Instance.InitialiseComponent(problemObjects, TypeHierarchy.Instance.GetLeafNodes());

            // Initial State
            var initialState = ProtobufReader.ReadInit(problemParser);

            InitBlock.Instance.InitialiseComponent(initialState);
        }
        #endregion
    }
}

