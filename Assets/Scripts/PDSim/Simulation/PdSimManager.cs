using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using PDSim.Simulation.Data;
using PDSim.Animation;
using Unity.VisualScripting;

namespace PDSim.Simulation 
{
    public class PdSimManager : MonoBehaviour
    {
        public CustomTypes types;
        public Fluents fluents;
        public Actions actions;
        public Plan plan;
        public Problem problem;
        
        public List<GameObject> objs;
        public List<GameObject> objs2;

        public ScriptMachine at;

        private void Start()
        { 
            
            //StartCoroutine(Simulate());
            //EventBus.Trigger(EventNames.actionEffectEvent, new EffectEventArgs("At", objs2.ToArray()));
            Debug.Log("ActionEffectEvent: At(robot, cell)");
            EventBus.Register<string>(EventNames.actionEffectEnd, i =>
            {
                Debug.Log("RECEIVED " + i);
            });
            EventBus.Trigger(EventNames.actionEffectStart, new EffectEventArgs("At(robot, cell)", objs.ToArray()));
        }

        private void OnActionEffectEnd(string obj)
        {
            Debug.Log("OnActionEffectEnd: " + obj);
        }
	}
}