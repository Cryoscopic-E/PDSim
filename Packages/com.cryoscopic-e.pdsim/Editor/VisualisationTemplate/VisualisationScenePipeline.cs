using PDSim.Components;
using PDSim.Helpers;
using PDSim.ScriptableObjects;
using PDSim.Utils;
using Proto;
using UnityEditor;
using UnityEditor.SceneTemplate;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PDSim.SceneTemplates
{
    public class VisualisationScenePipeline : SceneTemplatePipelineAdapter
    {

        public override bool IsValidTemplateForInstantiation(SceneTemplateAsset sceneTemplateAsset)
        {
            // Don't want user create this scene using "New Scene" in Unity
            return false;
        }

        public override void AfterTemplateInstantiation(SceneTemplateAsset sceneTemplateAsset, Scene scene, bool isAdditive, string sceneName)
        {
            // Connect data assets to simulation manager
            var simulationManager = Controller.Instance;
            var simulationDataRoot = AssetUtils.GetSimulationDataPath(scene.name);
            simulationManager.problem = AssetUtils.GetAsset<PlanningProblem>(simulationDataRoot + "/Problem.asset");
            simulationManager.planGeneration = AssetUtils.GetAsset<PlanGeneration>(simulationDataRoot + "/Plan.asset");


            EditorUtility.SetDirty(simulationManager);

            var problemParser = Problem.Parser.ParseFrom(simulationManager.problem.proto);
            var planParser = PlanGenerationResult.Parser.ParseFrom(simulationManager.planGeneration.proto);


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
    }
}

