using System.Collections;
using System.Collections.Generic;
using UnityEditor.SceneTemplate;
using UnityEngine.SceneManagement;
using PDSim.Simulation;
using PDSim.Utils;
using PDSim.Simulation.Data;
using UnityEditor;

public class SimulationScenePipeline : SceneTemplatePipelineAdapter
{

    public override bool IsValidTemplateForInstantiation(SceneTemplateAsset sceneTemplateAsset)
    {
        // Don't want user create this scene using "New Scene" in Unity
        return false;
    }

    public override void AfterTemplateInstantiation(SceneTemplateAsset sceneTemplateAsset, Scene scene, bool isAdditive, string sceneName)
    {
        // Connect data assets to simulation manager
        var simulationManager = PdSimManager.Instance;
        var simulationDataRoot = AssetUtils.GetSimulationDataPath(sceneName);

        simulationManager.types = AssetUtils.GetAsset<CustomTypes>(simulationDataRoot + "CustomTypes.asset");
        simulationManager.fluents = AssetUtils.GetAsset<Fluents>(simulationDataRoot + "Fluents.asset");
        simulationManager.actions = AssetUtils.GetAsset<Actions>(simulationDataRoot + "Actions.asset");
        simulationManager.plan = AssetUtils.GetAsset<Plan>(simulationDataRoot + "Plan.asset");
        simulationManager.problem = AssetUtils.GetAsset<Problem>(simulationDataRoot + "Problem.asset");

        EditorUtility.SetDirty(simulationManager);

        // setup animations
        simulationManager.SetUpAnimations();

        // TODO: setup objects
    }



}
