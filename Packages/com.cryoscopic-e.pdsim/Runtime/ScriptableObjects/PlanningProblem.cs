using UnityEngine;

namespace PDSim.ScriptableObjects
{
    public class PlanningProblem : ScriptableObject
    {
        [SerializeField]
        [HideInInspector]
        public byte[] proto;
    }
}
