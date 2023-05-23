using System;
using System.Collections.Generic;
using Unity.VisualScripting;

namespace PDSim.Components
{
    [Serializable, Inspectable]
    public class PdObject
    {

        public string name;
        public string type = "object";
        public List<string> childrenTypes;

        public PdObject(string name, string type)
        {
            this.name = name;
            this.type = type;
        }

        public PdObject(string name, string type, List<string> childrenTypes)
        {
            this.name = name;
            this.type = type;
            this.childrenTypes = childrenTypes;
        }


        public override string ToString()
        {
            return name + " - " + type;
        }
    }
}