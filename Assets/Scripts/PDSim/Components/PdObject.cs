using System;
using Unity.VisualScripting;

namespace PDSim.Components
{
    [Serializable, Inspectable]
    public class PdObject
    {
        
        public string name;
        public string type = "object";
        
        public override string ToString()
        {
            return name + " - " + type;
        }
    }
}