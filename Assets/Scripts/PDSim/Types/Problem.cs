using System;
using Unity.VisualScripting;

namespace PDSim.Types
{
    [Inspectable, Serializable]
    public class PDProblem
    {
        [Inspectable]
        public State initialState;
        [Inspectable]
        public State goalState;
    }
}
