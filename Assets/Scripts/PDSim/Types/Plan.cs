using System;
using System.Collections.Generic;
using Unity.VisualScripting;

namespace PDSim.Types
{
    [Serializable, Inspectable]
    public class PDPlan
    {
        [Inspectable]
        public List<PDAction> actions;
    }

}
