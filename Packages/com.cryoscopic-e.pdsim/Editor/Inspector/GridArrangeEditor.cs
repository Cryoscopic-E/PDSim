using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using PDSim.Helpers;

namespace PDSim.Editor.Inspector
{
    /// <summary>
    /// Custom editor for <see cref="Helper_GridArrange"/>.
    /// Provides a scene-view slot preview via <c>Handles</c> and a one-click
    /// <b>Arrange</b> button with full Undo support.
    /// </summary>
    [CustomEditor(typeof(Helper_GridArrange))]
    public class GridArrangeEditor : UnityEditor.Editor
    {
        #region Unity Lifecycle

        /// <summary>
        /// Draws the custom inspector GUI for GridArrange.
        /// </summary>
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            EditorGUILayout.Space(6);

            var arrange = (Helper_GridArrange)target;
            List<Transform> targets = arrange.GetEffectiveTargets();
            int count = targets.Count;

            // Info line: tell the user which targets will be moved
            string sourceLabel = (arrange.Targets != null && arrange.Targets.Count > 0)
                ? $"Using {count} explicit target(s) from the Targets list."
                : $"Using {count} VisualisationObject child(ren) (Targets list is empty).";
            EditorGUILayout.HelpBox(sourceLabel, MessageType.None);

            using (new EditorGUI.DisabledScope(count == 0))
            {
                if (GUILayout.Button("Arrange", GUILayout.Height(28)))
                {
                    if (count == 0)
                    {
                        EditorUtility.DisplayDialog(
                            "Grid Arrange",
                            "No targets found. Populate the Targets list or add VisualisationObject children.",
                            "OK");
                        return;
                    }

                    // Record transforms before the operation for Undo.
                    var undoTargets = new Object[count];
                    for (int i = 0; i < count; i++)
                        undoTargets[i] = targets[i];

                    Undo.RecordObjects(undoTargets, "Grid Arrange");

                    arrange.Arrange();

                    foreach (var t in targets)
                        EditorUtility.SetDirty(t);

                    Debug.Log($"[PDSim] GridArrange: arranged {count} object(s) in '{arrange.Mode}' mode.");
                }
            }

            EditorGUILayout.HelpBox(
                "Arrange moves targets to their computed slot positions.\n" +
                "The scene-view gizmo shows a preview of the slot layout.\n" +
                "The operation is undoable (Ctrl+Z).",
                MessageType.None);
        }

        private void OnSceneGUI()
        {
            var arrange = (Helper_GridArrange)target;
            List<Transform> targets = arrange.GetEffectiveTargets();
            int count = Mathf.Max(targets.Count, 1); // Show at least 1 slot as preview
            List<Vector3> slots = arrange.ComputeSlots(count);

            // Draw slot markers and connecting lines using Handles
            Color wireColor = new Color(0.2f, 0.8f, 1f, 0.8f);
            Color lineColor = new Color(0.2f, 0.8f, 1f, 0.35f);
            float capRadius  = HandleUtility.GetHandleSize(arrange.transform.position) * 0.12f;

            GUIStyle indexStyle = new GUIStyle
            {
                normal = { textColor = wireColor },
                fontStyle = FontStyle.Bold,
                fontSize = 10
            };

            for (int i = 0; i < slots.Count; i++)
            {
                // Sphere marker at each slot
                Handles.color = wireColor;
                Handles.SphereHandleCap(0, slots[i], Quaternion.identity,
                    capRadius, EventType.Repaint);

                // Slot index label
                Handles.Label(slots[i] + Vector3.up * (capRadius * 1.4f),
                    i.ToString(), indexStyle);

                // Connecting line to previous slot
                if (i > 0)
                {
                    Handles.color = lineColor;
                    Handles.DrawLine(slots[i - 1], slots[i]);
                }
            }

            // Origin marker
            Vector3 origin = arrange.transform.position + arrange.OriginOffset;
            Handles.color = new Color(1f, 0.7f, 0f, 0.9f);
            Handles.DrawWireCube(origin, Vector3.one * capRadius * 0.6f);

            // Origin offset drag handle
            EditorGUI.BeginChangeCheck();
            Vector3 newOrigin = Handles.PositionHandle(origin, Quaternion.identity);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(arrange, "Move GridArrange Origin");
                arrange.OriginOffset = newOrigin - arrange.transform.position;
                EditorUtility.SetDirty(arrange);
            }
        }

        #endregion
    }
}
