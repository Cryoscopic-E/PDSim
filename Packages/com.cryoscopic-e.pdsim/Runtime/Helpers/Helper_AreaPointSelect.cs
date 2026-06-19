using System.Collections.Generic;
using UnityEngine;

namespace PDSim.Helpers
{
    /// <summary>
    /// Defines a 3D bounding volume and returns random points within it.
    /// Supports collision-aware sampling via <see cref="TryGetNonCollidingPoint"/> and
    /// <see cref="GetNonCollidingPoints"/> to produce non-overlapping positions.
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

        [Header("Collision Avoidance")]
        [Tooltip("Layer mask used when checking for colliders (default: Everything).")]
        public LayerMask ObstacleMask = ~0;

        [Tooltip("Sphere radius used to detect collider overlap when sampling collision-free points.")]
        [Min(0f)]
        public float ClearanceRadius = 0.5f;

        [Tooltip("Maximum rejection-sampling attempts per requested point before giving up.")]
        [Min(1)]
        public int MaxAttempts = 30;

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
        /// Returns a random point within the bounding volume with no collision check.
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

        /// <summary>
        /// Tries to find a random point inside the volume that does not overlap any scene collider.
        /// Uses the component's <see cref="ClearanceRadius"/> and <see cref="ObstacleMask"/>.
        /// </summary>
        /// <param name="point">
        /// The accepted world-space point, or <see cref="Vector3.zero"/> if no clear point was found.
        /// </param>
        /// <returns>
        /// True if a collision-free point was found within <see cref="MaxAttempts"/> tries.
        /// </returns>
        public bool TryGetNonCollidingPoint(out Vector3 point)
            => TryGetNonCollidingPoint(out point, ClearanceRadius, ObstacleMask);

        /// <summary>
        /// Tries to find a random point inside the volume that does not overlap any scene collider,
        /// using the supplied <paramref name="radius"/> and <paramref name="mask"/> instead of the
        /// component defaults.
        /// </summary>
        /// <param name="point">The accepted world-space point, or <see cref="Vector3.zero"/> on failure.</param>
        /// <param name="radius">Sphere radius for the overlap check.</param>
        /// <param name="mask">Layer mask for the overlap check.</param>
        /// <returns>True if a clear point was found within <see cref="MaxAttempts"/> tries.</returns>
        public bool TryGetNonCollidingPoint(out Vector3 point, float radius, LayerMask mask)
        {
            for (int i = 0; i < MaxAttempts; i++)
            {
                Vector3 candidate = GetRandomPoint();
                if (!Physics.CheckSphere(candidate, radius, mask, QueryTriggerInteraction.Ignore))
                {
                    point = candidate;
                    return true;
                }
            }

            point = Vector3.zero;
            return false;
        }

        /// <summary>
        /// Returns up to <paramref name="count"/> collision-free, mutually-spaced points inside the volume.
        /// Each candidate is tested with <see cref="Physics.CheckSphere"/> against scene colliders,
        /// and must be at least <paramref name="minSpacing"/> away from every already-accepted point
        /// (cheap Poisson-disk rejection).
        /// </summary>
        /// <param name="count">Number of points to produce.</param>
        /// <param name="minSpacing">
        /// Minimum world-space distance between any two accepted points.
        /// Pass a value less than 0 to default to <c>ClearanceRadius × 2</c>.
        /// </param>
        /// <returns>
        /// List of accepted world-space points (may contain fewer than <paramref name="count"/>
        /// entries if the volume is too densely packed).
        /// </returns>
        public List<Vector3> GetNonCollidingPoints(int count, float minSpacing = -1f)
        {
            float spacing = minSpacing < 0f ? ClearanceRadius * 2f : minSpacing;
            var accepted = new List<Vector3>(count);

            for (int n = 0; n < count; n++)
            {
                bool placed = false;

                for (int attempt = 0; attempt < MaxAttempts; attempt++)
                {
                    Vector3 candidate = GetRandomPoint();

                    // Reject if overlapping a scene collider.
                    if (Physics.CheckSphere(candidate, ClearanceRadius, ObstacleMask, QueryTriggerInteraction.Ignore))
                        continue;

                    // Reject if too close to an already-accepted point.
                    bool tooClose = false;
                    for (int j = 0; j < accepted.Count; j++)
                    {
                        if (Vector3.Distance(candidate, accepted[j]) < spacing)
                        {
                            tooClose = true;
                            break;
                        }
                    }
                    if (tooClose) continue;

                    accepted.Add(candidate);
                    placed = true;
                    break;
                }

                if (!placed)
                    Debug.LogWarning(
                        $"[PDSim] AreaPointSelect '{name}': could not place point {n + 1}/{count} " +
                        $"after {MaxAttempts} attempts. Try enlarging the area, reducing ClearanceRadius, " +
                        $"or lowering the point count.");
            }

            return accepted;
        }

        #endregion
    }
}
