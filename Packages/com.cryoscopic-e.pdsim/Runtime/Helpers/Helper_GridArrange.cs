using System.Collections.Generic;
using UnityEngine;
using PDSim.Components;

namespace PDSim.Helpers
{
    /// <summary>
    /// Arrange mode for <see cref="Helper_GridArrange"/>.
    /// </summary>
    public enum GridArrangeMode
    {
        /// <summary>Place objects in a 2-D grid on the XZ plane.</summary>
        Grid,
        /// <summary>Place objects in a single row along X.</summary>
        Row,
        /// <summary>Stack objects vertically along Y.</summary>
        Stack
    }

    /// <summary>
    /// Arranges a set of <see cref="VisualisationObject"/>s into a regular grid, row, or
    /// vertical stack with a single editor click.
    ///
    /// Useful for blocks-world / logistics / grid domains where objects start on an
    /// integer lattice. Works alongside <see cref="Helper_SnapToGrid"/> — if a grid
    /// helper is assigned the computed slot positions are passed through its
    /// <see cref="Helper_SnapToGrid.SnapPoint"/> rounding.
    ///
    /// If <see cref="Targets"/> is empty the component uses all immediate
    /// <see cref="VisualisationObject"/> children of this GameObject.
    ///
    /// The scene-view gizmo previews slot positions before you commit. Press
    /// <b>Arrange</b> in the Inspector to apply (fully undoable).
    /// </summary>
    public class Helper_GridArrange : MonoBehaviour
    {
        #region Inspector Fields

        [Header("Layout")]
        [Tooltip("How to distribute objects: 2-D grid (XZ plane), single row (X), or vertical stack (Y).")]
        public GridArrangeMode Mode = GridArrangeMode.Grid;

        [Tooltip("Number of columns per row (Grid mode only). Rows wrap automatically.")]
        [Min(1)]
        public int Columns = 3;

        [Tooltip("Per-slot spacing vector.\n" +
                 "Grid: X = column step, Z = row step.\n" +
                 "Row:  X = slot step.\n" +
                 "Stack: Y = layer step.")]
        public Vector3 Spacing = new Vector3(1f, 1f, 1f);

        [Tooltip("Offset from this transform's world position to the first slot.")]
        public Vector3 OriginOffset;

        [Header("Targets")]
        [Tooltip("Explicit list of transforms to arrange. If empty, all immediate " +
                 "VisualisationObject children of this GameObject are used.")]
        public List<Transform> Targets = new List<Transform>();

        [Header("Grid (optional)")]
        [Tooltip("When assigned, each computed slot position is snapped through this grid helper.")]
        public Helper_SnapToGrid Grid;

        #endregion

        #region Public API

        /// <summary>
        /// Computes the world-space slot positions for <paramref name="count"/> objects
        /// using the current <see cref="Mode"/>, <see cref="Spacing"/>, <see cref="Columns"/>,
        /// and <see cref="OriginOffset"/> settings.
        /// </summary>
        /// <param name="count">Number of slots to compute.</param>
        /// <returns>List of world-space slot positions (length == <paramref name="count"/>).</returns>
        public List<Vector3> ComputeSlots(int count)
        {
            var slots = new List<Vector3>(count);
            Vector3 origin = transform.position + OriginOffset;
            int cols = Mathf.Max(1, Columns);

            for (int i = 0; i < count; i++)
            {
                Vector3 pos;
                switch (Mode)
                {
                    case GridArrangeMode.Row:
                        pos = origin + new Vector3(i * Spacing.x, 0f, 0f);
                        break;

                    case GridArrangeMode.Stack:
                        pos = origin + new Vector3(0f, i * Spacing.y, 0f);
                        break;

                    case GridArrangeMode.Grid:
                    default:
                        int col = i % cols;
                        int row = i / cols;
                        pos = origin + new Vector3(col * Spacing.x, 0f, row * Spacing.z);
                        break;
                }

                if (Grid != null)
                    pos = Grid.SnapPoint(pos);

                slots.Add(pos);
            }

            return slots;
        }

        /// <summary>
        /// Moves each target transform to its computed slot position.
        /// Targets are taken from <see cref="Targets"/> if populated, otherwise from all
        /// immediate <see cref="VisualisationObject"/> children.
        ///
        /// Undo registration is handled by the editor companion; this method just performs
        /// the moves so it can also be called at runtime.
        /// </summary>
        public void Arrange()
        {
            List<Transform> targets = GetEffectiveTargets();

            if (targets.Count == 0)
            {
                Debug.LogWarning("[PDSim] GridArrange: no targets to arrange.");
                return;
            }

            List<Vector3> slots = ComputeSlots(targets.Count);

            for (int i = 0; i < targets.Count; i++)
                targets[i].position = slots[i];
        }

        /// <summary>
        /// Returns the resolved target list — <see cref="Targets"/> (minus nulls) when
        /// non-empty, otherwise all immediate <see cref="VisualisationObject"/> children.
        /// Exposed so the editor companion can register them for Undo before the operation.
        /// </summary>
        public List<Transform> GetEffectiveTargets()
        {
            // Prefer explicit list
            if (Targets != null && Targets.Count > 0)
            {
                var result = new List<Transform>(Targets.Count);
                for (int i = 0; i < Targets.Count; i++)
                {
                    if (Targets[i] != null)
                        result.Add(Targets[i]);
                }
                return result;
            }

            // Fall back to VisualisationObject children
            var children = new List<Transform>();
            for (int i = 0; i < transform.childCount; i++)
            {
                var child = transform.GetChild(i);
                if (child.GetComponent<VisualisationObject>() != null)
                    children.Add(child);
            }
            return children;
        }

        #endregion

        #region Editor Gizmos

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            List<Transform> targets = GetEffectiveTargets();
            int previewCount = Mathf.Max(targets.Count, 1);
            List<Vector3> slots = ComputeSlots(previewCount);

            Color slotColor = new Color(0.2f, 0.8f, 1f, 0.6f);
            Gizmos.color = slotColor;

            for (int i = 0; i < slots.Count; i++)
            {
                Gizmos.DrawWireSphere(slots[i], 0.1f);
                if (i > 0)
                    Gizmos.DrawLine(slots[i - 1], slots[i]);
            }
        }
#endif

        #endregion
    }
}
