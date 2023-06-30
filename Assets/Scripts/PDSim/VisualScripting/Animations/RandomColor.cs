using Unity.VisualScripting;
using UnityEngine;

namespace PDSim.Animations
{
    [UnitCategory("PDSim/Animations")]
    public class RandomColor : Unit
    {
        [DoNotSerialize]
        public ControlInput inputTrigger;

        [DoNotSerialize]
        public ControlOutput outputTrigger;


        [DoNotSerialize]
        public ValueInput renderer;



        protected override void Definition()
        {
            inputTrigger = ControlInput("inputTrigger", (flow) => { return SetColor(flow); });
            outputTrigger = ControlOutput("outputTrigger");
            renderer = ValueInput<Renderer>("renderer", null);
            Succession(inputTrigger, outputTrigger);
            Requirement(renderer, inputTrigger);
        }

        private ControlOutput SetColor(Flow flow)
        {
            var renderer = flow.GetValue<Renderer>(this.renderer);
            renderer.material.color = new Color(Random.value, Random.value, Random.value);
            return outputTrigger;
        }
    }
}