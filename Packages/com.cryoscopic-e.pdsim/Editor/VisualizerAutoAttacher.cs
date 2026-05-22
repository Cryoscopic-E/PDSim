using UnityEditor;
using UnityEngine;
using PDSim.Components;
namespace PDSim.Editor
{
    /// <summary>
    /// Automatically attempts to attach compiled visualizer scripts to their corresponding scene objects.
    /// Runs on load and whenever the hierarchy changes.
    /// </summary>
    [InitializeOnLoad]
    public static class VisualizerAutoAttacher
    {
        #region Constructor
        static VisualizerAutoAttacher()
        {
            EditorApplication.delayCall += ScanAndAttach;
            // Also run when scripts are compiled
            AssemblyReloadEvents.afterAssemblyReload += ScanAndAttach;
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Scans the scene and attaches generated visualizers and behaviors to appropriate GameObjects.
        /// </summary>
        public static void ScanAndAttach()
        {
            var fluentAnimations = Object.FindObjectsByType<FluentAnimation>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            var simObjects = Object.FindObjectsByType<VisualisationObject>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            bool madeChanges = false;

            // Iterate through all identified fluent animations and attempt to bind their visualizer scripts.
            foreach (var fa in fluentAnimations)
            {
                if (fa.AnimationDataList == null) continue;

                foreach (var data in fa.AnimationDataList)
                {
                    if (data.SceneObjectReference != null && data.Visualizer == null && !string.IsNullOrEmpty(data.ScriptClassName))
                    {
                        if (TryAttachFluent(fa, data)) madeChanges = true;
                    }
                }
            }

            // Iterate through all action animations and attempt to bind their visualizer scripts.
            var actionAnimations = Object.FindObjectsByType<ActionAnimation>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            foreach (var aa in actionAnimations)
            {
                if (aa.AnimationDataList == null) continue;

                foreach (var data in aa.AnimationDataList)
                {
                    if (data.SceneObjectReference != null && data.Visualizer == null && !string.IsNullOrEmpty(data.ScriptClassName))
                    {
                        if (TryAttachAction(aa, data)) madeChanges = true;
                    }
                }
            }

            // Identify and attach custom behavior scripts to simulation objects based on their object type.
            var sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            var sanitizedSceneName = System.Text.RegularExpressions.Regex.Replace(sceneName, @"[^a-zA-Z0-9_]", "");

            foreach (var obj in simObjects)
            {
                if (string.IsNullOrEmpty(obj.ObjectType)) continue;

                var className = $"{PDSimAPI.Generators.ActionScriptGenerator.ToPascalCase(obj.ObjectType)}Behavior";
                var fullTypeName = $"PDSim.Generated.Behaviors.{sanitizedSceneName}.{className}";

                if (TryAttachBehavior(obj.gameObject, fullTypeName))
                {
                    madeChanges = true;
                }
            }

            if (madeChanges)
            {
                AssetDatabase.SaveAssets();
            }
        }
        #endregion

        #region Private Methods
        private static bool TryAttachAction(ActionAnimation context, ActionAnimation.AnimationData data)
        {
            var type = ResolveType(data.ScriptClassName)
                ?? ResolveType("GeneratedVisualizers." + data.ScriptClassName);
            if (type != null)
            {
                var component = data.SceneObjectReference.GetComponent(type) as MonoBehaviour;
                if (component == null)
                {
                    component = data.SceneObjectReference.AddComponent(type) as MonoBehaviour;
                }

                if (component != null)
                {
                    data.Visualizer = component;
                    EditorUtility.SetDirty(context);
                    Debug.Log($"[PDSim] Auto-attached action visualizer '{data.ScriptClassName}' to '{data.SceneObjectReference.name}'.");
                    return true;
                }
            }
            return false;
        }

        private static bool TryAttachFluent(FluentAnimation context, FluentAnimation.AnimationData data)
        {
            // scriptClassName is now stored as the fully-qualified type name for new animations.
            // Fall back to the legacy "GeneratedVisualizers." prefix for older scenes.
            var type = ResolveType(data.ScriptClassName)
                ?? ResolveType("GeneratedVisualizers." + data.ScriptClassName);
            if (type != null)
            {
                var component = data.SceneObjectReference.GetComponent(type) as MonoBehaviour;
                if (component == null)
                {
                    component = data.SceneObjectReference.AddComponent(type) as MonoBehaviour;
                }

                if (component != null)
                {
                    data.Visualizer = component;
                    EditorUtility.SetDirty(context);
                    Debug.Log($"[PDSim] Auto-attached visualizer '{data.ScriptClassName}' to '{data.SceneObjectReference.name}'.");
                    return true;
                }
            }
            return false;
        }

        private static bool TryAttachBehavior(GameObject target, string fullTypeName)
        {
            var type = ResolveType(fullTypeName);
            if (type != null)
            {
                var component = target.GetComponent(type);
                if (component == null)
                {
                    Undo.AddComponent(target, type);
                    Debug.Log($"[PDSim] Auto-attached behavior '{fullTypeName}' to '{target.name}'.");
                    return true;
                }
            }
            return false;
        }

        private static System.Type ResolveType(string fullName)
        {
            foreach (var assembly in System.AppDomain.CurrentDomain.GetAssemblies())
            {
                var type = assembly.GetType(fullName);
                if (type != null) return type;
            }
            return null;
        }
        #endregion
    }
}
