using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using PDSim.Components;
using UnityEngine;

namespace PDSim.Simulation
{
    /// <summary>
    /// Class representing a successfully parsed domain and problem file.
    /// </summary>
    public class PdSimEnvironment : ScriptableObject
    {
        // Parsed Objects
        // Can be loaded in scene variables
        public List<PdTypedPredicate> fluents;
        public List<PdAction> actions;
        public PdTypeTree typeTree;
        public PdProblem problem;
        public PdPlan plan;
        public string DomainText { get; set; }
        public string ProblemText { get; set; }

        public void CreateInstance( string domainText, string problemText, JObject responseJObject )
        {
            DomainText = domainText;
            ProblemText = problemText;
            
            // //Types
            // typeTree = new PdTypeTree();
            // typeTree.Populate(parsedPddl["types"] as JObject);
            
            //Predicates
            fluents = new List<PdTypedPredicate>();
            var predicates = responseJObject["predicates"];
            foreach (var k in predicates.Children<JProperty>())
            {
                var fluent = new PdTypedPredicate()
                {
                    name = k.Name,
                    parameters = new List<PdParameter>()
                };
                foreach (var v in k.Value["args"].Children<JProperty>())
                {
                    var param = new PdParameter()
                    {
                        name = v.Name,
                        type = v.Value.ToString()
                    };
                    fluent.parameters.Add(param);
                }
                fluents.Add(fluent);
            }
            // Actions

            actions = new List<PdAction>();
            var parsedActions = responseJObject["actions"];
            foreach (var k in parsedActions.Children<JProperty>())
            {
                var action = new PdAction
                {
                    name = k.Name,
                    parameters = new List<PdParameter>(),
                    effects = new List<PdBooleanPredicate>()
                };
                foreach (var v in k.Value["params"].Children<JProperty>())
                {
                    var param = new PdParameter()
                    {
                        name = v.Name,
                        type = v.Value.ToString()
                    };
                    action.parameters.Add(param);
                }
                foreach (var v in k.Value["effects"].Children<JObject>())
                {
                    var fluent = new PdBooleanPredicate
                    {
                        name = v["fluent"].ToString(),
                        value = !v["negated"].ToObject<bool>(),
                        attributes = new List<string>()
                    };
                    foreach (var a in v["args"])
                    {
                        fluent.attributes.Add(a.ToString());
                    }
                    action.effects.Add(fluent);
                }
                actions.Add(action);
            }
            //Initial State
            var initialState = new PdState();
            initialState.fluents = new List<PdBooleanPredicate>();
            var init = responseJObject["init"];
            foreach (var k in init.Children<JProperty>())
            {
                foreach (var v in k.Value.Children<JObject>())
                {
                    var fluent = new PdBooleanPredicate
                    {
                        name = k.Name,
                        attributes = new List<string>(),
                        value = v["value"].ToObject<bool>()
                    };

                    foreach (var a in v["args"])
                    {
                        fluent.attributes.Add(a.ToString());
                    }
                    
                    initialState.fluents.Add(fluent);
                }
            }
            
            //Objects
            var objects = responseJObject["objects"];
            var listOfObjects = objects.Children<JProperty>().Select(k => new PdObject { name = k.Name, type = k.Value.ToString() }).ToList();
            
            //Plan
            var parsedPlan = responseJObject["plan"]["actions"];
            Debug.Log(parsedPlan);
            plan = new PdPlan
            {
                actions = new List<PdPlanAction>()
            };
          
            foreach (var k in parsedPlan.Children<JObject>())
            {
                var planAction = new PdPlanAction
                {
                    name = k["action_name"].ToString(),
                    parameters = new List<string>()
                };
                Debug.Log(planAction.name);
                foreach (var v in k["parameters"].Children())
                {
                    planAction.parameters.Add(v.ToString());
                }
                plan.actions.Add(planAction);
            }
            
            //Problem
            problem = new PdProblem
            {
                initialState = initialState,
                objects = listOfObjects
            };

        }
    }
}