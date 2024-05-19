using Unity.VisualScripting;
using UnityEngine;

namespace PDSim.VisualScripting.Animations
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

        [DoNotSerialize]
        public ValueInput colorInput;

        [DoNotSerialize]
        public ValueInput useRandomColor;

        protected override void Definition()
        {
            inputTrigger = ControlInput("inputTrigger", (flow) => { return SetColor(flow); });
            outputTrigger = ControlOutput("outputTrigger");
            renderer = ValueInput<Renderer>("renderer", null);
            colorInput = ValueInput<Color>("color", Color.white); // Default color is white
            useRandomColor = ValueInput<bool>("useRandomColor", true); // Default is true for random color

            Succession(inputTrigger, outputTrigger);
            Requirement(renderer, inputTrigger);
            Requirement(colorInput, inputTrigger);
            Requirement(useRandomColor, inputTrigger);
        }

        private ControlOutput SetColor(Flow flow)
        {
            var renderer = flow.GetValue<Renderer>(this.renderer);
            bool randomizeColor = flow.GetValue<bool>(this.useRandomColor);

            // Step 3: Decide which color to set based on the boolean check
            Color colorToSet = randomizeColor ? new Color(Random.value, Random.value, Random.value) : flow.GetValue<Color>(this.colorInput);

            renderer.material.color = colorToSet;
            return outputTrigger;
        }
    }
}