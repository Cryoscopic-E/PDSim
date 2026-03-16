using UnityEngine;

namespace PDSim.Helpers
{
    /// <summary>
    /// Snaps a GameObject's position to a uniform grid.
    /// Useful for grid-world domains (Sokoban, blocks world, etc.).
    /// Call <see cref="Snap"/> from animation scripts, or enable
    /// <see cref="snapContinuously"/> to keep the object on the grid every frame.
    /// </summary>
    public class Helper_SnapToGrid : MonoBehaviour
    {
        [Tooltip("World-space cell size.")]
        public float gridSize = 1f;

        [Tooltip("Axes to snap. Uncheck Y to leave height free.")]
        public bool snapX = true;
        public bool snapY = false;
        public bool snapZ = true;

        [Tooltip("Snap immediately when the scene starts.")]
        public bool snapOnAwake = true;

        [Tooltip("Re-snap every frame (useful during NavMesh arrival).")]
        public bool snapContinuously = false;

        private void Awake()
        {
            if (snapOnAwake) Snap();
        }

        private void LateUpdate()
        {
            if (snapContinuously) Snap();
        }

        /// <summary>Snaps the object's position to the nearest grid cell.</summary>
        public void Snap()
        {
            if (gridSize <= 0f) return;
            Vector3 p = transform.position;
            transform.position = new Vector3(
                snapX ? Mathf.Round(p.x / gridSize) * gridSize : p.x,
                snapY ? Mathf.Round(p.y / gridSize) * gridSize : p.y,
                snapZ ? Mathf.Round(p.z / gridSize) * gridSize : p.z
            );
        }

        /// <summary>Returns the grid-snapped position without moving the object.</summary>
        public Vector3 GetSnappedPosition()
        {
            if (gridSize <= 0f) return transform.position;
            Vector3 p = transform.position;
            return new Vector3(
                snapX ? Mathf.Round(p.x / gridSize) * gridSize : p.x,
                snapY ? Mathf.Round(p.y / gridSize) * gridSize : p.y,
                snapZ ? Mathf.Round(p.z / gridSize) * gridSize : p.z
            );
        }

        /// <summary>
        /// Returns the snapped world position for an arbitrary world-space point,
        /// without modifying this object.
        /// </summary>
        public Vector3 SnapPoint(Vector3 worldPoint)
        {
            if (gridSize <= 0f) return worldPoint;
            return new Vector3(
                snapX ? Mathf.Round(worldPoint.x / gridSize) * gridSize : worldPoint.x,
                snapY ? Mathf.Round(worldPoint.y / gridSize) * gridSize : worldPoint.y,
                snapZ ? Mathf.Round(worldPoint.z / gridSize) * gridSize : worldPoint.z
            );
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (gridSize <= 0f) return;

            UnityEditor.Handles.color = new Color(0.4f, 0.8f, 1f, 0.25f);
            Vector3 p = transform.position;
            int lines = 10;
            float half = lines * gridSize * 0.5f;

            if (snapX && snapZ)
            {
                for (int i = -lines; i <= lines; i++)
                {
                    // Lines along Z
                    UnityEditor.Handles.DrawLine(
                        new Vector3(p.x + i * gridSize, p.y, p.z - half),
                        new Vector3(p.x + i * gridSize, p.y, p.z + half));
                    // Lines along X
                    UnityEditor.Handles.DrawLine(
                        new Vector3(p.x - half, p.y, p.z + i * gridSize),
                        new Vector3(p.x + half, p.y, p.z + i * gridSize));
                }
            }
        }
#endif
    }
}
