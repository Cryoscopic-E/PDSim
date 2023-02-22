using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;

namespace PDSim.Components
{
    
    [Serializable, Inspectable]
    public class PdPredicate
    {
        [Inspectable]
        public string name;

        [Inspectable]
        public List<string> attributes;
    }
    
    [Serializable, Inspectable]
    public class PdTypedPredicate
    {
        [Inspectable]
        public string name;

        [Inspectable]
        public List<PdParameter> parameters;
        
        public override string ToString()
        {
            // format: name-type1-type2-...
            return parameters.Aggregate(name, (current, parameter) => current + ("-" + parameter.type));
        }
    }
    
    [Serializable, Inspectable]
    public class PdBooleanPredicate : PdPredicate
    {
        [Inspectable]
        public bool value;
    }
}

