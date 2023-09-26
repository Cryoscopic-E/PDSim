using System.Collections.Generic;
using UnityEngine;

namespace PDSim.Protobuf
{
    public class PdSimProblem : ScriptableObject
    {
        public string domainName;
        public string problemName;
        public List<string> features;
        public PdSimTypesDeclaration typesDeclaration;
        public List<PdSimFluent> fluents;
        public List<PdSimInstantaneousAction> instantaneousActions;
        public List<PdSimDurativeAction> durativeActions;
    }
}