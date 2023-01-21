using System;
using Unity.VisualScripting;

namespace PDSim.Components
{
    [Serializable, Inspectable]
    public class PdParameter
    {
        public string name;
        public string type;
    }
}