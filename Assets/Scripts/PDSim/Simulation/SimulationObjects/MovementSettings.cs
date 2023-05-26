using UnityEngine;

namespace PDSim.Simulation
{
    [CreateAssetMenu(fileName = "MovementSetting", menuName = "PdSim Agents", order = 0)]
    public class MovementSettings : ScriptableObject
    {
        public float speed = 1f;
        public float angularSpeed = 120f;
        public float acceleration = 8f;
        public float stoppingDistance = 0.1f;
        public bool faceTarget = false;
    }
}