using UnityEngine;
using Unity.VisualScripting;



namespace PDSim.Animations
{
    [UnitCategory("PDSim/Animations")]
    public class PlayParticles : Unit
    {
        [DoNotSerialize]
        public ControlInput inputTrigger;

        [DoNotSerialize]
        public ControlOutput outputTrigger;

        [DoNotSerialize]
        public ValueInput particleSystem;

        [DoNotSerialize]
        public ValueInput particlePosition;

        [DoNotSerialize]
        public ValueInput particleRotation;


        protected override void Definition()
        {
            inputTrigger = ControlInput("inputTrigger", (flow) => { return Spawn(flow); });
            outputTrigger = ControlOutput("outputTrigger");
            
            particleSystem = ValueInput<GameObject>("particleSystem", null);
            
            particlePosition = ValueInput<Vector3>("position", Vector3.zero);
            particleRotation = ValueInput<Vector3>("rotation", Quaternion.identity.eulerAngles);
            
            Succession(inputTrigger, outputTrigger);
            Requirement(particleSystem, inputTrigger);

        }

        private ControlOutput Spawn(Flow flow)
        {
            var particleObject = flow.GetValue<GameObject>(this.particleSystem);
            var particleSystem = particleObject.GetComponent<ParticleSystem>();


            particleObject.transform.position = flow.GetValue<Vector3>(this.particlePosition);
            particleObject.transform.rotation = Quaternion.Euler(flow.GetValue<Vector3>(this.particleRotation));

            particleSystem.Play();
            return outputTrigger;
        }
    }
}
