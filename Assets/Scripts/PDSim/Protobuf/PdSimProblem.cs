using System.Collections.Generic;
using UnityEngine;

namespace PDSim.Protobuf
{
    public class PdSimProblem : ScriptableObject
    {
        public string domainName;
        public string problemName;
        public PdSimTypesDeclaration typesDeclaration;
        public List<PdSimFluent> fluents;
        public List<PdSimInstantaneousAction> instantActions;
        public List<PdSimDurativeAction> durativeActions;
        public List<PdSimObject> objects;
        public List<PdSimFluentAssignment> init;
    }
}