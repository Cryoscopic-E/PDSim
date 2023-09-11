using System;
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
        // //public List<PdSimAction> Actions { get; set; }
        public List<PdSimObject> objects;
    }
}