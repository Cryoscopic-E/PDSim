using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace PDSim.VisualScripting.Events
{
    /// <summary>
    /// Class for the action effect end event.
    /// When an effect is finished animating, this event is triggered.
    /// </summary>
    [UnitTitle("Action End Event")]
    [UnitCategory("Events/PDSim")]
    public class ActionEndEvent : Unit
    {
        // Connects to animation definition
        [DoNotSerialize]
        [PortLabelHidden]
        public ControlInput inputTrigger { get; private set; }

        // When triggered, notify the animation is finished
        [DoNotSerialize]
        public ControlOutput outputTrigger { get; private set; }


        [SerializeAs(nameof(ActionName))]
        private string _actionName;

        [SerializeAs(nameof(ActionName))]
        public string ActionName
        {
            get => _actionName;
            set => _actionName = value;
        }

        protected override void Definition()
        {

            inputTrigger = ControlInput(nameof(inputTrigger), Trigger);
            outputTrigger = ControlOutput("Action End");
            Succession(inputTrigger, outputTrigger);
        }

        private ControlOutput Trigger(Flow flow)
        {
            // Trigger the event to notify the animation is finished
            EventBus.Trigger(EventNames.actionEnd, ActionName);
            return outputTrigger;
        }
    }

}
