using UnityEngine;
using PDSim.Simulation.Data;

namespace PDSim.Simulation 
{
    [ExecuteAlways]
    public class PdSimManager : MonoBehaviour
    {
        public CustomTypes types;
        public Fluents fluents;
        public Actions actions;
        public Plan plan;
        public Problem problem;
        
        private void Start()
        { 
            
            
        }
	}
}