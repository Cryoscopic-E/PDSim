using Unity.VisualScripting;
using UnityEngine;

namespace PDSim.VisualScripting
{
    public class ActionEffectEndEvent : Unit
    {
        [DoNotSerialize]// Mandatory attribute, to make sure we don’t serialize data that should never be serialized.
        [PortLabelHidden]// Hide the port label, as we normally hide the label for default Input and Output triggers.
        public ControlInput inputTrigger { get; private set; }

        [DoNotSerialize]
        public ControlOutput outputTrigger { get; private set; }

        [DoNotSerialize]
        public ValueInput myValue;
        [DoNotSerialize]
        [PortLabelHidden]


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
            EventBus.Trigger(EventNames.actionEffectEnd, EffectName);
            return outputTrigger;
        }
    }
}
