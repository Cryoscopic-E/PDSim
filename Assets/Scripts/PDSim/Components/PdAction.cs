using System;
using System.Collections.Generic;
using Unity.VisualScripting;

namespace PDSim.Components
{
    [Serializable, Inspectable]
    public class PdAction
    {
        [Inspectable]
        public string name;
        [Inspectable]
        public List<PdObject> parameters;
        [Inspectable]
        public List<PdEffectPredicate> effects;

        public override string ToString()
        {
            // action in PDDL plan style
            var action = name + " (";
            foreach (var attribute in parameters)
            {
                action += attribute.name + ", ";
            }
            action = action.Substring(0, action.Length - 2);
            action += ")";
            return action;
        }
    }

    [Serializable, Inspectable]
    public class PdPlanAction
    {
        [Inspectable]
        public string name;

        [Inspectable]
        public List<string> parameters;

        public override string ToString()
        {
            // action in PDDL plan style
            var action = name + " (";
            foreach (var attribute in parameters)
            {
                action += attribute + ", ";
            }
            action = action.Substring(0, action.Length - 2);
            action += ")";
            return action;
        }

    }
}
