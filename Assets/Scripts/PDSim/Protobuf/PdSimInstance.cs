using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PDSim.Protobuf
{
    public class PdSimInstance : ScriptableObject
    {
        public List<PdSimObject> objects;
        public List<PdSimFluentAssignment> init;
        public List<PdSimActionInstance> plan;
    }
}
