using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using PDSim.Helpers;

namespace PDSim.Editor.Inspector
{
    /// <summary>
    /// Custom editor for <see cref="Helper_ObjectScatter"/>.
    /// Provides a one-click <b>Scatter Children</b> button with full Undo support.
    /// </summary>
    [CustomEditor(typeof(Helper_ObjectScatter))]
    public class ObjectScatterEditor : UnityEditor.Editor
    {
        #region Unity Lifecycle

        /// <summary>
        /// Draws the custom inspector GUI for ObjectScatter.
        /// </summary>
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            EditorGUILayout.Space(6);

            var scatter = (Helper_ObjectScatter)target;

            // Warn when required reference is missing
            if (scatter.Area == null)
            {
                EditorGUILayout.HelpBox(
                    "Assign a Helper_AreaPointSelect to the Area field before scattering.",
                    MessageType.Warning);
            }

            using (new EditorGUI.DisabledScope(scatter.Area == null))
            {
                if (GUILayout.Button("Scatter Children", GUILayout.Height(28)))
                {
                    List<Transform> children = scatter.CollectChildren();

                    if (children.Count == 0)
                    {
                        EditorUtility.DisplayDialog(
                            "Object Scatter",
                            "No VisualisationObject children found on this GameObject.",
                            "OK");
                        return;
                    }

                    // Record all child transforms before the operation so Ctrl+Z restores them.
                    var undoTargets = new Object[children.Count];
                    for (int i = 0; i < children.Count; i++)
                        undoTargets[i] = children[i];

                    Undo.RecordObjects(undoTargets, "Scatter Children");

                    scatter.ScatterChildren();

                    foreach (var t in children)
                        EditorUtility.SetDirty(t);

                    Debug.Log($"[PDSim] ObjectScatter: scattered {children.Count} object(s) " +
                              $"inside '{scatter.Area.name}'.");
                }
            }

            EditorGUILayout.HelpBox(
                "Scatter Children distributes all VisualisationObject children inside the " +
                "assigned Area using collision-free, mutually-spaced sampling.\n" +
                "The operation is undoable (Ctrl+Z).",
                MessageType.None);
        }

        #endregion
    }
}
