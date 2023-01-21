using System;
using System.Collections.Generic;
using Unity.VisualScripting;

namespace PDSim.Components
{
    [Serializable, Inspectable]
    public class PdState
    {
        [Inspectable]
        public List<PdBooleanPredicate> fluents;

    }
}