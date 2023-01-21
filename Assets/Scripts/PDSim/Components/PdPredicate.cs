using System;
using System.Collections.Generic;
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
    }
    
    [Serializable, Inspectable]
    public class PdBooleanPredicate : PdPredicate
    {
        [Inspectable]
        public bool value;
    }
}

