using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using PDSim.Components;

namespace PDSim.Simulation.Data
{
    public class Fluents : PdSimData
    {
        public List<PdTypedPredicate> fluents;

        public override void CreateInstance(JObject parsedPddl)
        {
            var typeTree = new PdTypeTree();
            typeTree.Populate(parsedPddl["types"] as JObject);


            fluents = new List<PdTypedPredicate>();
            var predicates = parsedPddl["predicates"];
            foreach (var k in predicates.Children<JProperty>())
            {
                var fluent = new PdTypedPredicate()
                {
                    name = k.Name,
                    parameters = new List<PdObject>()
                };
                foreach (var v in k.Value["args"].Children<JProperty>())
                {
                    var childrenTypes = typeTree.GetChildrenTypes(v.Value.ToString());
                    var param = new PdObject(v.Name, v.Value.ToString(), childrenTypes);
                    fluent.parameters.Add(param);
                }
                fluents.Add(fluent);
            }
        }
    }
}