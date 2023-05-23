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
        
        private void Start()
        { 
            
        }

        private void TriggerAnimation()
        {
            // TODO: Works! Implement this for simulate plan!
            //Debug.Log("ActionEffectEvent: At(robot, cell)");
            //EventBus.Register<string>(EventNames.actionEffectEnd, i =>
            //{
            //    Debug.Log("RECEIVED " + i);
            //});
            //EventBus.Trigger(EventNames.actionEffectStart, new EffectEventArgs("At(robot, cell)", objs.ToArray()));
            throw new System.NotImplementedException();
        }

        public void SetUpAnimations()
        {
            var animationsRootObject = GameObject.Find("Animations");
            if (animationsRootObject == null)
            {
                animationsRootObject = new GameObject("Animations");
            }

            foreach (var fluent in fluents.fluents)
            {
                var fluentAnimation = animationsRootObject.AddComponent<FluentAnimation>();
                fluentAnimation.fluentName = fluent.ToString();
                fluentAnimation.data = new List<FluentAnimationData>();
            }
        }
	}
}