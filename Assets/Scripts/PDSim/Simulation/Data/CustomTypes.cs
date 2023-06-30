using Newtonsoft.Json.Linq;
using PDSim.Components;
using UnityEngine;

namespace PDSim.Simulation.Data
{
    public class CustomTypes : PdSimData
    {
        public PdTypeTree typeTree;

        public override void CreateInstance(JObject parsedPddl)
        {
            typeTree = new PdTypeTree();
            typeTree.Populate(parsedPddl["types"] as JObject);
        }
    }
}