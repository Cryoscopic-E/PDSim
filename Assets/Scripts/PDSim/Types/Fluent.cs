using System;
using System.Collections.Generic;
using Unity.VisualScripting;

namespace PDSim.Types
{

    [Serializable, Inspectable]
    public class PDFluent
    {
        [Inspectable]
        public string name;

        [Inspectable]
        public Dictionary<string, string> attributes;
    }
}

