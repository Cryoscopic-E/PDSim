using Unity.VisualScripting;
using UnityEngine;

namespace PDSim.Animations
{
    [UnitCategory("PDSim/Animations")]
    public class PlaySoundEffect : Unit
    {
        [DoNotSerialize]
        public ControlInput inputTrigger;

        [DoNotSerialize]
        public ControlOutput outputTrigger;

        [DoNotSerialize]
        public ValueInput audioSource;

        [DoNotSerialize]
        public ValueInput audioClip;

        [DoNotSerialize]
        public ValueInput volume;

        [DoNotSerialize]
        public ValueInput pitch;



        protected override void Definition()
        {
            inputTrigger = ControlInput("inputTrigger", (flow) => { return PlayAudio(flow); });
            outputTrigger = ControlOutput("outputTrigger");

            audioSource = ValueInput<AudioSource>("audioSource", null);
            audioClip = ValueInput<AudioClip>("audioClip", null);

            volume = ValueInput<float>("volume", 1f);
            pitch = ValueInput<float>("pitch", .1f);

            Succession(inputTrigger, outputTrigger);

        }

        private ControlOutput PlayAudio(Flow flow)
        {
            var source = flow.GetValue<AudioSource>(this.audioSource);
            var clip = flow.GetValue<AudioClip>(this.audioClip);

            source.clip = clip;
            source.volume = flow.GetValue<float>(this.volume);
            source.pitch = flow.GetValue<float>(this.pitch);
            source.Play();

            return outputTrigger;
        }
    }
}