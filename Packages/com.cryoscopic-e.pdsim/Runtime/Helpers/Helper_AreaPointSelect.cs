using UnityEngine;

namespace PDSim.Helpers
{
    /// <summary>
    /// Defines a 3D bounding volume and returns random points within it.
    /// Use the Scene view handles to resize and reposition interactively.
    /// </summary>
    public class Helper_AreaPointSelect : MonoBehaviour
    {
        public Vector3 size = Vector3.one;
        public Vector3 offset;

        [Header("Gizmo")]
        public Color gizmoColor = new Color(0f, 1f, 0.8f, 1f);
        public bool showLabel = true;

        public Vector3 Center => transform.position + offset;

        public Vector3 GetRandomPoint()
        {
            Vector3 c = Center;
            return new Vector3(
                Random.Range(c.x - size.x * 0.5f, c.x + size.x * 0.5f),
                Random.Range(c.y - size.y * 0.5f, c.y + size.y * 0.5f),
                Random.Range(c.z - size.z * 0.5f, c.z + size.z * 0.5f)
            );
        }

        /// <summary>
        /// Returns true if the world-space point lies inside the volume.
        /// </summary>
        public bool Contains(Vector3 worldPoint)
        {
            Vector3 local = worldPoint - Center;
            return Mathf.Abs(local.x) <= size.x * 0.5f
                && Mathf.Abs(local.y) <= size.y * 0.5f
                && Mathf.Abs(local.z) <= size.z * 0.5f;
        }
    }
}
