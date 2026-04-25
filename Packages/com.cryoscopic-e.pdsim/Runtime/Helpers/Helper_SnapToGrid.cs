using UnityEngine;
using UnityEngine.Serialization;

namespace PDSim.Helpers
{
    /// <summary>
    /// Snaps a GameObject's position to a uniform grid.
    /// Useful for grid-world domains (Sokoban, blocks world, etc.).
    /// Call <see cref="Snap"/> from animation scripts, or enable
    /// <see cref="SnapContinuously"/> to keep the object on the grid every frame.
    /// </summary>
    public class Helper_SnapToGrid : MonoBehaviour
    {
        #region Inspector Fields

        [Header("Settings")]
        [Tooltip("World-space cell size.")]
        [SerializeField, FormerlySerializedAs("gridSize")]
        private float GridSize = 1f;

        [Header("Axes")]
        [Tooltip("Axes to snap. Uncheck Y to leave height free.")]
        [SerializeField, FormerlySerializedAs("snapX")]
        private bool SnapX = true;

        [SerializeField, FormerlySerializedAs("snapY")]
        private bool SnapY = false;

        [SerializeField, FormerlySerializedAs("snapZ")]
        private bool SnapZ = true;

        [Header("Execution")]
        [Tooltip("Snap immediately when the scene starts.")]
        [SerializeField, FormerlySerializedAs("snapOnAwake")]
        private bool SnapOnAwake = true;

        [Tooltip("Re-snap every frame (useful during NavMesh arrival).")]
        [SerializeField, FormerlySerializedAs("snapContinuously")]
        private bool SnapContinuously = false;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            if (SnapOnAwake) Snap();
        }

        private void LateUpdate()
        {
            if (SnapContinuously) Snap();
        }

        #endregion

        #region Public API

        /// <summary>Snaps the object's position to the nearest grid cell.</summary>
        public void Snap()
        {
            if (GridSize <= 0f) return;
            Vector3 p = transform.position;
            transform.position = new Vector3(
                SnapX ? Mathf.Round(p.x / GridSize) * GridSize : p.x,
                SnapY ? Mathf.Round(p.y / GridSize) * GridSize : p.y,
                SnapZ ? Mathf.Round(p.z / GridSize) * GridSize : p.z
            );
        }

        /// <summary>Returns the grid-snapped position without moving the object.</summary>
        public Vector3 GetSnappedPosition()
        {
            if (GridSize <= 0f) return transform.position;
            Vector3 p = transform.position;
            return new Vector3(
                SnapX ? Mathf.Round(p.x / GridSize) * GridSize : p.x,
                SnapY ? Mathf.Round(p.y / GridSize) * GridSize : p.y,
                SnapZ ? Mathf.Round(p.z / GridSize) * GridSize : p.z
            );
        }

        /// <summary>
        /// Returns the snapped world position for an arbitrary world-space point,
        /// without modifying this object.
        /// </summary>
        public Vector3 SnapPoint(Vector3 worldPoint)
        {
            if (GridSize <= 0f) return worldPoint;
            return new Vector3(
                SnapX ? Mathf.Round(worldPoint.x / GridSize) * GridSize : worldPoint.x,
                SnapY ? Mathf.Round(worldPoint.y / GridSize) * GridSize : worldPoint.y,
                SnapZ ? Mathf.Round(worldPoint.z / GridSize) * GridSize : worldPoint.z
            );
        }

        #endregion

        #region Editor Gizmos

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (GridSize <= 0f) return;

            UnityEditor.Handles.color = new Color(0.4f, 0.8f, 1f, 0.25f);
            Vector3 p = transform.position;
            int lines = 10;
            float half = lines * GridSize * 0.5f;

            if (SnapX && SnapZ)
            {
                for (int i = -lines; i <= lines; i++)
                {
                    // Lines along Z
                    UnityEditor.Handles.DrawLine(
                        new Vector3(p.x + i * GridSize, p.y, p.z - half),
                        new Vector3(p.x + i * GridSize, p.y, p.z + half));
                    // Lines along X
                    UnityEditor.Handles.DrawLine(
                        new Vector3(p.x - half, p.y, p.z + i * GridSize),
                        new Vector3(p.x + half, p.y, p.z + i * GridSize));
                }
            }
        }
#endif

        #endregion
    }
}
