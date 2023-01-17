using UnityEditor.SceneTemplate;
using UnityEngine.SceneManagement;

public class PdSimScenePipeline : ISceneTemplatePipeline
{
    public bool IsValidTemplateForInstantiation(SceneTemplateAsset sceneTemplateAsset)
    {
        // Don't want user create this scene using "New Scene" in Unity
        return false;
    }

    public void BeforeTemplateInstantiation(SceneTemplateAsset sceneTemplateAsset, bool isAdditive, string sceneName)
    {

    }

    public void AfterTemplateInstantiation(SceneTemplateAsset sceneTemplateAsset, Scene scene, bool isAdditive, string sceneName)
    {

    }
}
