using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PDSim.Protobuf
{
    /// <summary>
    /// A PdSim instance
    /// 
    /// This is the PDDL problem instance representation
    /// </summary>
    public class PdSimInstance : ScriptableObject
    {
        public List<PdSimObject> objects;
        public List<PdSimFluentAssignment> init;
        public List<PdSimActionInstance> plan;
    }
}
