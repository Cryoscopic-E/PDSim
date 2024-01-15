using PDSim.Protobuf;
using PDSim.Simulation;
using PDSim.Utils;
using UnityEditor;
using UnityEditor.SceneTemplate;
using UnityEngine.SceneManagement;

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
        var simulationDataRoot = AssetUtils.GetSimulationDataPath(scene.name);
        simulationManager.problemModel = AssetUtils.GetAsset<PdSimProblem>(simulationDataRoot + "/PdSimProblem.asset");
        simulationManager.problemInstance = AssetUtils.GetAsset<PdSimInstance>(simulationDataRoot + "/PdSimInstance.asset");


        EditorUtility.SetDirty(simulationManager);

        simulationManager.SetUpAnimations();

        simulationManager.SetUpObjects();
    }



}
