using System.Collections;
using System.Collections.Generic;
using PDSim.Protobuf;
using PDSim.Simulation.SimulationObject;
using Unity.VisualScripting;
using UnityEngine;

namespace PDSim.VisualScripting.Events
{
    /// <summary>
    /// Class for the action event.
    /// When an action is scheduled to be animated, this event is triggered.
    /// </summary>
    [UnitTitle("Action Event")]
    [UnitCategory("Events/PDSim")]
    public class ActionEvent : EventUnit<ActionEventArgs>
    {
        protected override bool register => true;

        // Total number of arguments of the action
        [SerializeAs(nameof(ArgumentCount))]
        private int _argumentCount;

        // Name of the action to be animated (match PDDL "action-name")
        [SerializeAs(nameof(ActionName))]
        private string _actionName;

        // List of arguments of the action (match PDDL "object-type")
        [SerializeAs(nameof(ActionArguments))]
        private List<string> _actionArguments;

        [DoNotSerialize]
        public int ArgumentCount
        {
            get => _argumentCount;
            set => _argumentCount = Mathf.Clamp(value, 0, 10);
        }

        [SerializeAs(nameof(ActionName))]
        public string ActionName
        {
            get => _actionName;
            set => _actionName = value;
        }

        [DoNotSerialize]
        public List<string> ActionArguments
        {
            get => _actionArguments;
            set => _actionArguments = value;
        }

        [DoNotSerialize]
        public List<ValueOutput> ArgumentPorts { get; } = new List<ValueOutput>();

        [DoNotSerialize]
        public ValueOutput StartTime;

        [DoNotSerialize]
        public ValueOutput Duration;

        protected override void Definition()
        {
            base.Definition();
            coroutine = true;

            ArgumentPorts.Clear();

            for (var i = 0; i < ArgumentCount; i++)
            {
                ArgumentPorts.Add(ValueOutput<GameObject>(ActionArguments[i]));
            }



        }

        public override EventHook GetHook(GraphReference reference)
        {
            return new EventHook(EventNames.actionStart);
        }


        protected override bool ShouldTrigger(Flow flow, ActionEventArgs args)
        {
            return string.Compare(ActionName, args.actionName) == 0;
        }

        protected override void AssignArguments(Flow flow, ActionEventArgs args)
        {
            // Set attributes value
            for (var i = 0; i < ArgumentCount; i++)
            {
                flow.SetValue(ArgumentPorts[i], args.arguments[i]);
            }

            flow.SetValue(StartTime, args.startTime);

            flow.SetValue(Duration, args.duration);

        }
    }


    public struct ActionEventArgs
    {
        public readonly string actionName;

        public readonly PdSimSimulationObject[] arguments;

        public readonly float startTime;

        public readonly float duration;

        public ActionEventArgs(PdSimActionInstance pdSimActionInstance, params PdSimSimulationObject[] arguments)
        {
            actionName = pdSimActionInstance.name;
            this.arguments = arguments;
            startTime = pdSimActionInstance.startTime;
            duration = pdSimActionInstance.endTime - pdSimActionInstance.startTime;
        }
    }

}
