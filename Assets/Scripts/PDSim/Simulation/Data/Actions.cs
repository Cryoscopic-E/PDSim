using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using PDSim.Components;
using UnityEngine;

namespace PDSim.Simulation.Data
{
    public class Actions : PdSimData
    {
        public List<PdAction> actions;


        public override void CreateInstance(JObject parsedPddl)
        {
            actions = new List<PdAction>();
            var parsedActions = parsedPddl["actions"];
            foreach (var k in parsedActions.Children<JProperty>())
            {
                var action = new PdAction
                {
                    name = k.Name,
                    parameters = new List<PdObject>(),
                    effects = new List<PdEffectPredicate>()
                };
                foreach (var v in k.Value["params"].Children<JProperty>())
                {
                    var param = new PdObject(v.Name, v.Value.ToString());
                    action.parameters.Add(param);
                }
                foreach (var v in k.Value["effects"].Children<JObject>())
                {
                    var fluent = new PdEffectPredicate
                    {
                        name = v["fluent"].ToString(),
                        value = !v["negated"].ToObject<bool>(),
                        attributes = new List<string>(),
                        parameterMapping = new List<int>()
                    };
                    foreach (var a in v["args"])
                    {
                        fluent.attributes.Add(a.ToString());
                        int index = action.parameters.FindIndex(x => x.name == a.ToString());
                        fluent.parameterMapping.Add(index);
                    }
                    action.effects.Add(fluent);
                }
                actions.Add(action);
            }
        }
    }
}