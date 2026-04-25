using UnityEngine;

namespace PDSim.ScriptableObjects
{
    /// <summary>
    /// Scriptable object that stores the generated plan in protobuf format.
    /// </summary>
    public class PlanGeneration : ScriptableObject
    {
        /// <summary>
        /// The serialized protobuf data of the generated plan.
        /// </summary>
        [SerializeField]
        [HideInInspector]
        public byte[] Proto;
    }
}
