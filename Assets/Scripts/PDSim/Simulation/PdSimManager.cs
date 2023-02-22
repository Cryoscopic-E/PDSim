using System;
using System.Collections.Generic;
using PDSim.Animation;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;

namespace PDSim.Simulation
{
    public class PdSimManager : MonoBehaviour
    {
        [HideInInspector]
        public PdSimEnvironment environment;
        
        public string simulationName;
        
        
        private Dictionary<string, PdSimSimulationObject> _pdSimObjects;
        
        
        private void Start()
        {
            _pdSimObjects = new Dictionary<string, PdSimSimulationObject>();
            
            // Add all children GameObjects to the dictionary
            foreach (var pdSimObject in FindObjectsOfType<PdSimSimulationObject>())
            {
                _pdSimObjects.Add(pdSimObject.name, pdSimObject);
            }
        }
        
        private void Simulate()
        {
            // Go through each action in the plan
            foreach (var planAction in environment.plan.actions)
            {
                
            }

            // Event trigger
            // EventBus.Trigger(EventNames.actionEffectEvent, new EffectEventArgs("At-Robot-Cell", t.testEffectObjects));
            
        }

        public PdSimSimulationObject GetPdSimObject(string objName)
        {
            return _pdSimObjects[objName];
        }
        
    }
    
    [Serializable]
    public struct EffectAnimation
    {
        
    }
}