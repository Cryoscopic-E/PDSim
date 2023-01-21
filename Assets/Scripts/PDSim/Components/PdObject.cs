using System;
using Unity.VisualScripting;

namespace PDSim.Components
{
    [Serializable, Inspectable]
    public class PdObject
    {
        [Inspectable]
        public string name;
        
        [Inspectable]
        public string type = "object";
    }
}