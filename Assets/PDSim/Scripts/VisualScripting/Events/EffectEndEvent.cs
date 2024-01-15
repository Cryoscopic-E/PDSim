using Unity.VisualScripting;
using UnityEngine;

namespace PDSim.VisualScripting.Events
{
    /// <summary>
    /// Class for the action effect end event.
    /// When an effect is finished animating, this event is triggered.
    /// </summary>
    [UnitTitle("Effect End Event")]
    [UnitCategory("Events/PDSim")]
    public class EffectEndEvent : Unit
    {
        // Connects to animation definition
        [DoNotSerialize]
        [PortLabelHidden]
        public ControlInput inputTrigger { get; private set; }

        // When triggered, notify the animation is finished
        [DoNotSerialize]
        public ControlOutput outputTrigger { get; private set; }


        [SerializeAs(nameof(EffectName))]
        private string _effectName;

        [SerializeAs(nameof(EffectName))]
        public string EffectName
        {
            get => _effectName;
            set => _effectName = value;
        }

        protected override void Definition()
        {

            inputTrigger = ControlInput(nameof(inputTrigger), Trigger);
            outputTrigger = ControlOutput("AnimationEnd");
            Succession(inputTrigger, outputTrigger);
        }

        private ControlOutput Trigger(Flow flow)
        {
            // Trigger the event to notify the animation is finished
            EventBus.Trigger(EventNames.actionEffectEnd, EffectName);
            return outputTrigger;
        }
    }
}
