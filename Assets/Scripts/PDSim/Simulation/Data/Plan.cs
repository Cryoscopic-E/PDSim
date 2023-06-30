using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using PDSim.Components;
using Unity.VisualScripting;

namespace PDSim.Simulation.Data
{
    public class Plan : PdSimData
    {
        [Inspectable]
        public List<PdPlanAction> actions;

        public override void CreateInstance(JObject parsedPddl)
        {
            
            actions = new List<PdPlanAction>();

            foreach (var action in parsedPddl["plan"]["actions"])
            {
                var planAction = new PdPlanAction
                {
                    name = action["action_name"].ToString(),
                    parameters = new List<string>()
                };
                foreach (var param in action["parameters"])
                {
                    planAction.parameters.Add(param.ToString());
                }
                actions.Add(planAction);
            }
        }
    }

}
