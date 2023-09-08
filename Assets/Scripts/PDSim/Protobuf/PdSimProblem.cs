using System;
using System.Collections.Generic;
using UnityEngine;

namespace PDSim.Protobuf
{
    [Serializable]
    public class PdSimProblem : ScriptableObject
    {

        public string DomainName { get; set; }
        public string ProblemName { get; set; }
        public PdSimTypesDeclaration TypesDeclaration { get; set; }

        public List<PdSimFluent> Fluents { get; set; }
        public List<PdSimAction> Actions { get; set; }
        public List<PdSimObject> Objects { get; set; }
    }
}