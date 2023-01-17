using System;
using System.Collections.Generic;
using Unity.VisualScripting;

namespace PDSim.Types
{
    [Serializable, Inspectable]
    public class PDState
    {
        [Inspectable]
        public List<PDFluent> fluents;

    }
}