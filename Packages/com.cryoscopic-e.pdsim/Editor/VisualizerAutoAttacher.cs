using UnityEditor;
using UnityEngine;
using PDSim.Components;
using System.Collections.Generic;
using System.Linq;

namespace PDSim.Editor
{
    /// <summary>
    /// Automatically attempts to attach compiled visualizer scripts to their corresponding scene objects.
    /// Runs on load and whenever the hierarchy changes.
    /// </summary>
    [InitializeOnLoad]
    public static class VisualizerAutoAttacher
    {
        static VisualizerAutoAttacher()
        {
            EditorApplication.delayCall += ScanAndAttach;
            // Also run when scripts are compiled
            AssemblyReloadEvents.afterAssemblyReload += ScanAndAttach;
        }

        public static void ScanAndAttach()
        {
            var fluentAnimations = Object.FindObjectsByType<FluentAnimation>(FindObjectsSortMode.None);
            var simObjects = Object.FindObjectsByType<VisualisationObject>(FindObjectsSortMode.None);
            bool madeChanges = false;

            // --- Attach Fluent Visualizers ---
            foreach (var fa in fluentAnimations)
            {
                if (fa.animationData == null) continue;

                foreach (var data in fa.animationData)
                {
                    if (data.sceneObjectReference != null && data.visualizer == null && !string.IsNullOrEmpty(data.scriptClassName))
                    {
                        if (TryAttachFluent(fa, data)) madeChanges = true;
                    }
                }
            }

            // --- Attach Object Behaviors ---
            var sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            var sanitizedSceneName = System.Text.RegularExpressions.Regex.Replace(sceneName, @"[^a-zA-Z0-9_]", "");

            foreach (var obj in simObjects)
            {
                if (string.IsNullOrEmpty(obj.objectType)) continue;

                var className = $"{PDSimAPI.Generators.ActionScriptGenerator.ToPascalCase(obj.objectType)}Behavior";
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

        private static bool TryAttachFluent(FluentAnimation context, FluentAnimation.AnimationData data)
        {
            var type = ResolveType("GeneratedVisualizers." + data.scriptClassName);
            if (type != null)
            {
                var component = data.sceneObjectReference.GetComponent(type) as MonoBehaviour;
                if (component == null)
                {
                    component = data.sceneObjectReference.AddComponent(type) as MonoBehaviour;
                }

                if (component != null)
                {
                    data.visualizer = component;
                    EditorUtility.SetDirty(context);
                    Debug.Log($"[PDSim] Auto-attached visualizer '{data.scriptClassName}' to '{data.sceneObjectReference.name}'.");
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
    }
}
