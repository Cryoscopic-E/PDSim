using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using PDSim.Components;
using PDSim.Interactive;
using PDSim.Interactive.UI;

namespace PDSim.Editor.Interactive
{
    public static class InteractiveSceneSetup
    {
        [MenuItem("PDSim/Create Interactive Planning Scene")]
        public static void CreateScene()
        {
            // Create a new scene
            var newScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            
            // 1. Basic Environment
            var camGo = new GameObject("Main Camera");
            var cam = camGo.AddComponent<Camera>();
            camGo.tag = "MainCamera";
            camGo.transform.position = new Vector3(0, 5, -10);
            camGo.transform.LookAt(Vector3.zero);

            var lightGo = new GameObject("Directional Light");
            var light = lightGo.AddComponent<Light>();
            light.type = LightType.Directional;
            lightGo.transform.rotation = Quaternion.Euler(50, -30, 0);

            // 2. Managers
            var managerGo = new GameObject("PDSim Managers");
            managerGo.AddComponent<PDSimWorldObserver>();
            managerGo.AddComponent<Animations>();
            managerGo.AddComponent<ProblemObjects>();
            managerGo.AddComponent<TypeHierarchy>();
            
            // Add UI Dashboard
            managerGo.AddComponent<UnityEngine.UIElements.UIDocument>();
            managerGo.AddComponent<InteractiveDashboard>();
            
            // 3. Controller (for animation handoff)
            var controllerGo = new GameObject("PDSim Controller");
            controllerGo.AddComponent<Controller>();
            controllerGo.AddComponent<AnimationsController>();

            // Focus on the managers
            Selection.activeGameObject = managerGo;
            
            Debug.Log("[PDSim] Created new Interactive Planning Scene. Add PDSimMetadata, SemanticSensors, and LogicalActions to your objects.");
        }
    }
}
