using global::Unity.VisualScripting;
using System.Collections.Generic;
using UnityEngine;

namespace PDSim.Animation
{
    public class ActionEffectEvent : EventUnit<EffectEventArgs>
    {
        protected override bool register => true;

        [SerializeAs(nameof(ArgumentCount))]
        private int _argumentCount;

        [SerializeAs(nameof(IsNegative))]
        private bool _isNegative;

        [SerializeAs(nameof(EffectName))]
        private string _effectName;

        [SerializeAs(nameof(EffectArguments))]
        private List<string> _effectArguments; // Match PDDL "object-type"

        [DoNotSerialize]
        [Inspectable, UnitHeaderInspectable("Arguments")]
        public int ArgumentCount
        {
            get => _argumentCount;
            set => _argumentCount = Mathf.Clamp(value, 0, 10);
        }

        [DoNotSerialize]
        [Inspectable, UnitHeaderInspectable("Negative Effect")]
        public bool IsNegative
        {
            get => _isNegative;
            set => _isNegative = value;
        }

        [DoNotSerialize]
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

        /// <summary>
        /// The name of the event.
        /// </summary>
        [DoNotSerialize]
        public ValueInput Name { get; private set; }

        [DoNotSerialize]
        public List<ValueOutput> ArgumentPorts { get; } = new List<ValueOutput>();


        protected override void Definition()
        {
            base.Definition();

            Name = ValueInput(nameof(Name), EffectName);

            ArgumentPorts.Clear();

            for (var i = 0; i < ArgumentCount; i++)
            {
                ArgumentPorts.Add(ValueOutput<GameObject>(EffectArguments[i]));
            }
        }

        public override EventHook GetHook(GraphReference reference)
        {
            return new EventHook(EventNames.actionEffectEvent);
        }


        protected override bool ShouldTrigger(Flow flow, EffectEventArgs args)
        {
            return CompareNames(flow, Name, args.name);
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
            EventBus.Trigger(EventNames.actionEffectEvent, target, new CustomEventArgs(name, args));
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