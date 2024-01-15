using System.Collections.Generic;
using PDSim.Protobuf;
using Unity.VisualScripting;
using UnityEngine;

namespace PDSim.VisualScripting.Events
{
    /// <summary>
    /// Class for the action effect event.
    /// When an effect is scheduled to be animated, this event is triggered.
    /// </summary>
    [UnitTitle("Effect Event")]
    [UnitCategory("Events/PDSim")]
    public class EffectEvent : EventUnit<EffectEventArgs>
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

        // Value Type of the effect
        [SerializeAs(nameof(EffectValueType))]
        private ValueType _effectValueType;

        [DoNotSerialize]
        public ValueType EffectValueType
        {
            get => _effectValueType;
            set => _effectValueType = value;
        }

        [DoNotSerialize]
        public ValueOutput Value;

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

            if (EffectValueType == ValueType.Boolean)
            {
                Value = ValueOutput<bool>("Boolean");
            }
            else if (EffectValueType == ValueType.Real || EffectValueType == ValueType.Int)
            {
                Value = ValueOutput<float>("Number");
            }
            else
            {
                Value = ValueOutput<string>("Symbol");
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
            // Set attributes value
            for (var i = 0; i < ArgumentCount; i++)
            {
                flow.SetValue(ArgumentPorts[i], args.arguments[i]);
            }
            // Set value
            var contentCase = args.value.contentCase;
            switch (contentCase)
            {
                default:
                case Atom.ContentOneofCase.Symbol:
                    flow.SetValue(Value, args.value.valueSymbol);
                    break;
                case Atom.ContentOneofCase.Int:
                case Atom.ContentOneofCase.Real:
                    flow.SetValue(Value, float.Parse(args.value.valueSymbol));
                    break;
                case Atom.ContentOneofCase.Boolean:
                    flow.SetValue(Value, bool.Parse(args.value.valueSymbol));
                    break;
            }

        }
    }


    public struct EffectEventArgs
    {
        public readonly string name;

        public readonly PdSimAtom value;

        public readonly GameObject[] arguments;

        public EffectEventArgs(string name, PdSimAtom value, params GameObject[] arguments)
        {
            this.name = name;
            this.value = value;
            this.arguments = arguments;
        }
    }
}