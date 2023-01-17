using System;
using System.Collections.Generic;
using Unity.VisualScripting;

namespace PDSim.Types
{
    [Serializable, Inspectable]
    public class PDAction
    {
        [Inspectable]
        public string name;
        [Inspectable]
        public Dictionary<string, string> attributes;
        [Inspectable]
        public List<PDBooleanFluent> effects;

        public override string ToString()
        {
            // action in PDDL plan style
            string action = name + " (";
            foreach (var attribute in attributes)
            {
                action += attribute.Key + ", ";
            }
            action = action.Substring(0, action.Length - 2);
            action += ")";
            return action;
        }
    }
}
