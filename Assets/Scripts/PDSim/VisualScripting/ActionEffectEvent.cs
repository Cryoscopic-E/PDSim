using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace PDSim.VisualScripting
{
    /// <summary>
    /// Class for the action effect event.
    /// When an effect is scheduled to be animated, this event is triggered.
    /// </summary>
    [UnitTitle("Action Effect Event")]
    [UnitCategory("Events/PDSim")]
    public class ActionEffectEvent : EventUnit<EffectEventArgs>
    {
        protected override bool register => true;

        // Total number of arguments of the effect
        [SerializeAs(nameof(ArgumentCount))]
        private int _argumentCount;

        // Name of the effect to be animated (match PDDL "action-name")
        [SerializeAs(nameof(EffectName))]
        private string _effectName;

        // List of arguments of the effect (match PDDL "object-type")
        [SerializeAs(nameof(EffectArguments))]
        private List<string> _effectArguments;

        [DoNotSerialize]
        public int ArgumentCount
        {
            get => _argumentCount;
            set => _argumentCount = Mathf.Clamp(value, 0, 10);
        }

        [SerializeAs(nameof(EffectName))]
        public string EffectName
        {
            get => _effectName;
            set => _effectName = value;
        }

        [DoNotSerialize]
        public List<string> EffectArguments
        {
            get => _effectArguments;
            set => _effectArguments = value;
        }

        [DoNotSerialize]
        public List<ValueOutput> ArgumentPorts { get; } = new List<ValueOutput>();


        protected override void Definition()
        {
            base.Definition();
            coroutine = true;

            //Name = ValueInput(nameof(Name), EffectName);

            ArgumentPorts.Clear();

            for (var i = 0; i < ArgumentCount; i++)
            {
                ArgumentPorts.Add(ValueOutput<GameObject>(EffectArguments[i]));
            }
        }

        public override EventHook GetHook(GraphReference reference)
        {
            return new EventHook(EventNames.actionEffectStart);
        }


        protected override bool ShouldTrigger(Flow flow, EffectEventArgs args)
        {
            return string.Compare(EffectName, args.name) == 0;
        }

        protected override void AssignArguments(Flow flow, EffectEventArgs args)
        {
            for (var i = 0; i < ArgumentCount; i++)
            {
                flow.SetValue(ArgumentPorts[i], args.arguments[i]);
            }
        }

        public static void Trigger(GameObject target, string name, params GameObject[] args)
        {
            EventBus.Trigger(EventNames.actionEffectStart, target, new CustomEventArgs(name, args));
        }
    }


    public struct EffectEventArgs
    {
        public readonly string name;

        public readonly GameObject[] arguments;

        public EffectEventArgs(string name, params GameObject[] arguments)
        {
            this.name = name;
            this.arguments = arguments;
        }
    }
}