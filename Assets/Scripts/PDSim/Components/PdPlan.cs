using System;
using System.Collections.Generic;
using Unity.VisualScripting;

namespace PDSim.Components
{
    [Serializable, Inspectable]
    public class PdPlan
    {
        [Inspectable]
        public List<PdPlanAction> actions;
    }

}
