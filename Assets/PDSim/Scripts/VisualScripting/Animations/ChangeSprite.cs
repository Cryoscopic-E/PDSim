using Unity.VisualScripting;
using UnityEngine;

namespace PDSim.VisualScripting.Animations
{
    /// <summary>
    /// Animation unit to change the sprite of a renderer
    /// </summary>
    [UnitCategory("PDSim/Animations")]
    public class ChangeSprite : Unit
    {
        [DoNotSerialize]
        public ControlInput inputTrigger;

        [DoNotSerialize]
        public ControlOutput outputTrigger;


        [DoNotSerialize]
        public ValueInput renderer;

        [DoNotSerialize]
        public ValueInput sprite;

        protected override void Definition()
        {
            inputTrigger = ControlInput("inputTrigger", ChangeSpriteTexture);
            outputTrigger = ControlOutput("outputTrigger");
            renderer = ValueInput<Renderer>("renderer", null);
            sprite = ValueInput<Sprite>("sprite", null);
            Succession(inputTrigger, outputTrigger);
            Requirement(renderer, inputTrigger);
            Requirement(sprite, inputTrigger);
        }

        private ControlOutput ChangeSpriteTexture(Flow flow)
        {
            var renderer = flow.GetValue<SpriteRenderer>(this.renderer);

            // Check if the renderer is null
            if (renderer == null)
            {
                Debug.LogError("No SpriteRenderer");
                return outputTrigger;
            }

            renderer.sprite = flow.GetValue<Sprite>(this.sprite);
            return outputTrigger;
        }
    }
}