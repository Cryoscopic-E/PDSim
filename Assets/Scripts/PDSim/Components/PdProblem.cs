using System;
using System.Collections.Generic;
using Unity.VisualScripting;

namespace PDSim.Components
{
    [Inspectable, Serializable]
    public class PdProblem
    {
        [Inspectable] 
        public List<PdObject> objects;
        [Inspectable]
        public PdState initialState;
        [Inspectable]
        public PdState goalState;
    }
}
