using UnityEngine;

namespace PDSim.ScriptableObjects
{
    /// <summary>
    /// ScriptableObject that stores the planning problem data in protobuf format.
    /// </summary>
    public class ParsedProblem : ScriptableObject
    {
        #region Fields

        /// <summary>
        /// The protobuf data representing the planning problem.
        /// </summary>
        [SerializeField]
        [HideInInspector]
        [Tooltip("The protobuf data representing the planning problem.")]
        public byte[] Proto;

        #endregion
    }
}
