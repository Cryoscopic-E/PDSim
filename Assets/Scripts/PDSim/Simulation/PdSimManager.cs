using UnityEngine;
using UnityEngine.Serialization;

namespace PDSim.Simulation
{
    public class PdSimManager : MonoBehaviour
    {
        public PdSimEnvironment environment;
        
        private void Start()
        { 
            Debug.Log(environment.typeTree.GetRoot().Name);
            
            
        }
        
        
    }
}