using Unity.VisualScripting;
using System.Collections.Generic;
using UnityEngine;

namespace PDSim.Animation
{
    [UnitCategory("Events/PDSim")]
    public class ActionEffectEvent : EventUnit<EffectEventArgs>
    {
        protected override bool register => true;

        [SerializeAs(nameof(ArgumentCount))]
        private int _argumentCount;

        [SerializeAs(nameof(EffectName))]
        private string _effectName;

        [SerializeAs(nameof(EffectArguments))]
        private List<string> _effectArguments; // Match PDDL "object-type"

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

        /// <summary>
        /// The name of the event.
        /// </summary>
        // [DoNotSerialize]
        // [PortLabelHidden]
        // protected ValueInput Name { get; private set;}

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
            return EffectName == args.name;
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