using UnityEngine;
using Unity.VisualScripting;

namespace PDSim.VisualScripting.Animations
{
    /// <summary>
    /// Animation unit to play a particle effect
    /// </summary>
    [UnitCategory("PDSim/Animations")]
    public class PlayPauseParticles : Unit
    {
        [DoNotSerialize]
        public ControlInput inputTrigger;

        [DoNotSerialize]
        public ControlOutput outputTrigger;

        [DoNotSerialize]
        public ValueInput particleSystem;

        [DoNotSerialize]
        public ValueInput play;

        [DoNotSerialize]
        public ValueInput changeColor;

        [DoNotSerialize]
        public ValueInput color;


        protected override void Definition()
        {
            inputTrigger = ControlInput("inputTrigger", (flow) => { return PlayOrPause(flow); });
            outputTrigger = ControlOutput("outputTrigger");

            particleSystem = ValueInput<GameObject>("particleSystem", null);

            play = ValueInput<bool>("play", true);


            changeColor = ValueInput<bool>("changeColor", false);
            color = ValueInput<Color>("color", Color.white); // Default color is white

            Succession(inputTrigger, outputTrigger);
            Requirement(particleSystem, inputTrigger);
            Requirement(color, inputTrigger);
            //Requirement(changeColor, inputTrigger);

        }

        private ControlOutput PlayOrPause(Flow flow)
        {
            var particleObject = flow.GetValue<GameObject>(this.particleSystem);
            var particleSystem = particleObject.GetComponent<ParticleSystem>();

            var shouldChangeColor = flow.GetValue<bool>(changeColor);


            if (shouldChangeColor)
            {
                var mainModule = particleSystem.main;
                mainModule.startColor = flow.GetValue<Color>(color);
            }


            if (flow.GetValue<bool>(this.play))
            {
                particleSystem.gameObject.SetActive(true);
                particleSystem.Play();
            }
            else
            {
                particleSystem.Stop();
                particleSystem.gameObject.SetActive(false);
            }



            return outputTrigger;
        }
    }
}
