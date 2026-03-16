using UnityEngine;

namespace PDSim.ScriptableObjects
{
    public class PlanGeneration : ScriptableObject
    {
        [SerializeField]
        [HideInInspector]
        public byte[] proto;
    }
}
