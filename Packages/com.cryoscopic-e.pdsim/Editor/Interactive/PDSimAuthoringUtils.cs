using UnityEditor;
using UnityEngine;
using PDSim.Components;
using PDSim.Interactive;

namespace PDSim.Editor.Interactive
{
    public static class PDSimAuthoringUtils
    {
        [MenuItem("GameObject/PDSim/Make Interactive Object", false, 10)]
        public static void MakeInteractive(MenuCommand menuCommand)
        {
            GameObject go = menuCommand.context as GameObject;
            if (go == null) return;

            Undo.AddComponent<PDSimMetadata>(go);
            var visObj = Undo.AddComponent<VisualisationObject>(go);
            visObj.objectType = "object";

            Debug.Log($"[PDSim] {go.name} is now an Interactive Object.");
        }

        [MenuItem("GameObject/PDSim/Add Semantic Sensor (Raycast)", false, 11)]
        public static void AddRaycastSensor(MenuCommand menuCommand)
        {
            GameObject go = menuCommand.context as GameObject;
            if (go == null) return;

            var sensor = Undo.AddComponent<SemanticSensor>(go);
            // Default to at[self, hit] as it's the most common
            Debug.Log($"[PDSim] Added Raycast Sensor to {go.name}. Configure 'Mapping Expression' in Inspector.");
        }

        [MenuItem("GameObject/PDSim/Add Logical Action", false, 12)]
        public static void AddLogicalAction(MenuCommand menuCommand)
        {
            GameObject go = menuCommand.context as GameObject;
            if (go == null) return;

            var action = Undo.AddComponent<LogicalAction>(go);
            action.actionName = "action_name";
            Debug.Log($"[PDSim] Added Logical Action to {go.name}. Define Preconditions and Effects in Inspector.");
        }

        [MenuItem("PDSim/Setup Interactive Managers", false, 20)]
        public static void SetupManagers()
        {
            var managerGo = GameObject.Find("PDSim Managers");
            if (managerGo == null)
            {
                managerGo = new GameObject("PDSim Managers");
                Undo.RegisterCreatedObjectUndo(managerGo, "Setup PDSim Managers");
            }

            if (managerGo.GetComponent<PDSimWorldObserver>() == null) Undo.AddComponent<PDSimWorldObserver>(managerGo);
            if (managerGo.GetComponent<Animations>() == null) Undo.AddComponent<Animations>(managerGo);
            if (managerGo.GetComponent<ProblemObjects>() == null) Undo.AddComponent<ProblemObjects>(managerGo);
            if (managerGo.GetComponent<TypeHierarchy>() == null) Undo.AddComponent<TypeHierarchy>(managerGo);
            
            Debug.Log("[PDSim] Interactive Managers verified/added to scene.");
        }
    }
}
