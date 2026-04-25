using UnityEngine;

namespace PDSim.ScriptableObjects
{
    /// <summary>
    /// Scriptable object to store movement settings for a simulation object.
    /// Defines parameters like speed, acceleration, and rotation behavior.
    /// </summary>
    [CreateAssetMenu(fileName = "MovementSetting", menuName = "PdSim/Movement Settings", order = 0)]
    public class MovementSettings : ScriptableObject
    {
        /// <summary>
        /// Movement speed of the object.
        /// </summary>
        [Tooltip("Movement speed of the object.")]
        public float Speed = 1f;

        /// <summary>
        /// Angular (rotation) speed of the object.
        /// </summary>
        [Tooltip("Angular (rotation) speed of the object.")]
        public float AngularSpeed = 120f;

        /// <summary>
        /// Acceleration of the object.
        /// </summary>
        [Tooltip("Acceleration of the object.")]
        public float Acceleration = 8f;

        /// <summary>
        /// The distance from the target at which the object stops.
        /// </summary>
        [Tooltip("The distance from the target at which the object stops.")]
        public float StoppingDistance = 0.1f;

        /// <summary>
        /// Whether the object should rotate to face its movement target.
        /// </summary>
        [Tooltip("Whether the object should rotate to face its movement target.")]
        public bool FaceTarget = false;
    }
}