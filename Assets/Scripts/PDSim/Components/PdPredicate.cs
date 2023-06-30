using System;
using System.Collections.Generic;
using Unity.VisualScripting;

namespace PDSim.Components
{

    [Serializable]
    public abstract class PdPredicate
    {
        [Inspectable]
        public string name;

        [Inspectable]
        public List<string> attributes;

        public override string ToString()
        {
            return name + " " + string.Join(",", attributes);
        }
    }

    [Serializable, Inspectable]
    public class PdBooleanPredicate : PdPredicate
    {
        [Inspectable]
        public bool value;

        public override string ToString()
        {
            return name + " (" + string.Join(",", attributes) + ") -> " + value;
        }
    }


    [Serializable, Inspectable]
    public class PdEffectPredicate : PdBooleanPredicate
    {
        [Inspectable]
        public List<int> parameterMapping;
    }



    [Serializable, Inspectable]
    public class PdTypedPredicate
    {
        [Inspectable]
        public string name;

        [Inspectable]
        public List<PdObject> parameters;

        public override string ToString()
        {
            return name + " (" + string.Join(", ", parameters) + ")";
        }
    }

}

