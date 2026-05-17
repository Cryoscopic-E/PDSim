using UnityEngine;
using UnityEngine.Serialization;

namespace PDSim.Helpers
{
    /// <summary>
    /// Defines a 3D bounding volume and returns random points within it.
    /// Use the Scene view handles to resize and reposition interactively.
    /// </summary>
    public class Helper_AreaPointSelect : MonoBehaviour
    {
        #region Inspector Fields

        [Header("Settings")]
        [Tooltip("The size of the bounding volume.")]
        public Vector3 Size = Vector3.one;

        [Tooltip("The offset from the transform position.")]
        public Vector3 Offset;

        [Header("Gizmo")]
        [Tooltip("The color of the gizmo in the Scene view.")]
        public Color GizmoColor = new Color(0f, 1f, 0.8f, 1f);

        [Tooltip("Whether to show the dimension label in the Scene view.")]
        public bool ShowLabel = true;

        #endregion

        #region Public API

        /// <summary>
        /// Gets the world-space center of the bounding volume.
        /// </summary>
        public Vector3 Center => transform.position + Offset;

        /// <summary>
        /// Returns a random point within the bounding volume.
        /// </summary>
        /// <returns>A random world-space point.</returns>
        public Vector3 GetRandomPoint()
        {
            Vector3 c = Center;
            return new Vector3(
                Random.Range(c.x - Size.x * 0.5f, c.x + Size.x * 0.5f),
                Random.Range(c.y - Size.y * 0.5f, c.y + Size.y * 0.5f),
                Random.Range(c.z - Size.z * 0.5f, c.z + Size.z * 0.5f)
            );
        }

        /// <summary>
        /// Returns true if the world-space point lies inside the volume.
        /// </summary>
        /// <param name="worldPoint">The world-space point to check.</param>
        /// <returns>True if the point is inside, false otherwise.</returns>
        public bool Contains(Vector3 worldPoint)
        {
            Vector3 local = worldPoint - Center;
            return Mathf.Abs(local.x) <= Size.x * 0.5f
                && Mathf.Abs(local.y) <= Size.y * 0.5f
                && Mathf.Abs(local.z) <= Size.z * 0.5f;
        }

        #endregion
    }
}
