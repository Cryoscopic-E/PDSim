using System.Collections.Generic;
using UnityEngine;
using PDSim.Components;

namespace PDSim.Helpers
{
    /// <summary>
    /// Distributes child <see cref="VisualisationObject"/>s within a
    /// <see cref="Helper_AreaPointSelect"/> volume, producing collision-free,
    /// mutually-spaced placements in a single editor click.
    ///
    /// Attach this component to the container that holds the visualisation objects
    /// (typically the <c>ProblemObjects</c> GameObject), assign an <see cref="Area"/>,
    /// then press <b>Scatter Children</b> in the Inspector.
    ///
    /// The operation is fully undoable from the editor.
    /// </summary>
    public class Helper_ObjectScatter : MonoBehaviour
    {
        #region Inspector Fields

        [Header("Area")]
        [Tooltip("The bounding volume to scatter objects within. Must have a Helper_AreaPointSelect.")]
        public Helper_AreaPointSelect Area;

        [Header("Spacing")]
        [Tooltip("Minimum world-space distance between any two placed objects. " +
                 "Values less than 0 use Area.ClearanceRadius × 2 automatically.")]
        [SerializeField]
        private float MinSpacing = -1f;

        [Header("Ground Snapping")]
        [Tooltip("If enabled, each placed object is snapped downward onto the first collider " +
                 "below it (useful for terrain-based scenes).")]
        [SerializeField]
        private bool SnapToGround = false;

        [Tooltip("Layer mask used for the ground raycast. Ignored when SnapToGround is off.")]
        [SerializeField]
        private LayerMask GroundMask = ~0;

        [Header("Rotation")]
        [Tooltip("Apply a random Y-axis rotation to each placed object.")]
        [SerializeField]
        private bool RandomYaw = true;

        [Header("Grid (optional)")]
        [Tooltip("When assigned, each placed position is snapped to the referenced grid after all " +
                 "other placement steps.")]
        [SerializeField]
        private Helper_SnapToGrid Grid;

        #endregion

        #region Public API

        /// <summary>
        /// Collects all immediate <see cref="VisualisationObject"/> children and distributes
        /// them inside <see cref="Area"/> using collision-free, mutually-spaced sampling.
        ///
        /// Call this from the editor (via the <c>ObjectScatterEditor</c> button) or from
        /// any runtime script that needs programmatic scatter. Undo registration is handled
        /// by the editor companion so it is not repeated here.
        /// </summary>
        public void ScatterChildren()
        {
            if (Area == null)
            {
                Debug.LogWarning("[PDSim] ObjectScatter: no Area assigned.");
                return;
            }

            var children = CollectChildren();

            if (children.Count == 0)
            {
                Debug.LogWarning("[PDSim] ObjectScatter: no VisualisationObject children found.");
                return;
            }

            List<Vector3> points = Area.GetNonCollidingPoints(children.Count, MinSpacing);

            int placed = Mathf.Min(children.Count, points.Count);
            for (int i = 0; i < placed; i++)
            {
                Vector3 pos = points[i];

                if (SnapToGround)
                    pos = SnapDown(pos);

                if (Grid != null)
                    pos = Grid.SnapPoint(pos);

                children[i].position = pos;

                if (RandomYaw)
                    children[i].rotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
            }

            if (placed < children.Count)
                Debug.LogWarning(
                    $"[PDSim] ObjectScatter: only {placed}/{children.Count} children could be placed. " +
                    "Enlarge the area, reduce MinSpacing, or lower MaxAttempts threshold on the Area.");
        }

        /// <summary>
        /// Returns the list of direct <see cref="VisualisationObject"/> child transforms
        /// that will be moved by <see cref="ScatterChildren"/>. Exposed so the editor
        /// companion can register them for Undo before the operation.
        /// </summary>
        public List<Transform> CollectChildren()
        {
            var result = new List<Transform>();
            for (int i = 0; i < transform.childCount; i++)
            {
                var child = transform.GetChild(i);
                if (child.GetComponent<VisualisationObject>() != null)
                    result.Add(child);
            }
            return result;
        }

        #endregion

        #region Implementation

        private Vector3 SnapDown(Vector3 pos)
        {
            const float castOriginOffset = 100f;
            const float castDistance     = 200f;

            if (Physics.Raycast(pos + Vector3.up * castOriginOffset, Vector3.down,
                    out RaycastHit hit, castDistance, GroundMask, QueryTriggerInteraction.Ignore))
            {
                return hit.point;
            }

            return pos; // No ground found — keep the sampled height
        }

        #endregion
    }
}
