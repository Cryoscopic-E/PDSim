using UnityEngine;
using System.Collections.Generic;
using PDSim.Simulation.Data;
using PDSim.Animation;
using Unity.VisualScripting;

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
        
        public List<GameObject> objs;

        private void Start()
        { 
            
            EventBus.Register<ActionEffectEvent>(EventNames.actionEffectEvent, OnActionEffectEvent);
            EventBus.Trigger(EventNames.actionEffectEvent, new EffectEventArgs("At-Robot-Cell", objs.ToArray()));
            
        }

        private void OnActionEffectEvent(ActionEffectEvent e)
        {
            Debug.Log("ActionEffectEvent: " + e.EffectName);
        }
	}
}