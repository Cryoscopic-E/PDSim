using System.Collections.Generic;
using UnityEngine;
using PDSim.PlanningModel;

namespace PDSim.Simulation
{
    /// <summary>
    /// A PdSim problem instance
    /// 
    /// This is the PDDL domain representation
    /// </summary>
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