using UnityEngine;
using System.Collections.Generic;
using PDDL.Parser;
using PDDL.Model.PDDL12;
using PDDL.Model.PDDL12.Effects;
using SimpleJSON;

public static class Parser
{
    public static void ParseDomain(string domainText, out List<PddlPredicate> predicates, out List<PddlAction> actions)
    {
        // Instancing the PDDL parser 
        var parser = new PDDL12Parser();

        var domainPredicates = new List<PddlPredicate>();
        var domainActions = new List<PddlAction>();

        # region DOMAIN PARSE

        try
        {
            // Get definitions list
            var list = parser.Parse(domainText);
            // Get Domain
            var domain = (Domain) list[0];

            /* CREATE PDDL PREDICATES*/
            // Loop through all predicates
            foreach (var predicate in domain.Predicates)
            {
                /* Predicate Parameters*/
                var predicateParams = new List<PddlObject>();
                // Loop through parameters list
                foreach (var parameter in predicate.Parameters)
                {
                    // parameter in form name-type added to parameters list
                    predicateParams.Add(new PddlObject(parameter.ToString(), parameter.Type.ToString()));
                }

                // add predicate to list
                domainPredicates.Add(new PddlPredicate(predicate.Name.ToString(), predicateParams));
            }

            /* CREATE PDDL ACTIONS*/
            foreach (var action in domain.Actions)
            {
                /* Action Parameters */
                var actionParams = new List<PddlObject>();
                // Loop through parameters list
                foreach (var parameter in action.Parameters)
                {
                    // parameter in form name-type added to parameters list
                    actionParams.Add(new PddlObject(parameter.Value.ToString(), parameter.Type.ToString()));
                }

                /* Action's Effects */
                var effects = new List<PddlEffect>();
                // Get effects list in PDDL (and (..)..) conjunction effect list of predicates
                var effect = (ConjunctionEffect) action.Effect;
                // Loop through effects list
                foreach (var predicate in effect.Effects)
                {
                    // select negation an regular predicate
                    // in PDDL not(...) if considered negated
                    if (predicate.Type == EffectKind.Negated)
                    {
                        var negatedPredicate = (NegatedEffect) predicate;
                        
                        var indexes = new List<int>();
                        foreach (var parameter in negatedPredicate.Effects.Parameters)
                        {
                            var t = actionParams.FindIndex(a => a.name.Equals(parameter.ToString()));
                            Debug.Log(t);
                            indexes.Add(actionParams.FindIndex(a => a.name.Equals(parameter.ToString())));
                        }
                        effects.Add(new PddlEffect(negatedPredicate.Effects.Name.Value, true, indexes));
                    }
                    else
                    {
                        var regularPredicate = (RegularEffect) predicate;
                        var indexes = new List<int>();
                        foreach (var parameter in regularPredicate.Effects.Parameters)
                        {
                            indexes.Add(actionParams.FindIndex(a => a.name.Equals(parameter.ToString())));
                        }
                        effects.Add(new PddlEffect(regularPredicate.Effects.Name.Value, false, indexes));
                        
                    }
                }

                // add action to list
                domainActions.Add(new PddlAction(action.Functor.Value, actionParams, effects));
            }
        }
        catch (PDDLSyntaxException pe)
        {
            Debug.LogError("Domain Syntax Error::" + pe.Message);
        }

        #endregion

        predicates = domainPredicates;
        actions = domainActions;
    }


    public static void ParseProblem(string problemText, out List<string> pTypes, out List<PddlObject> objects,
        out List<PddlInit> initPredicates)
    {
        // Instancing the PDDL parser 
        var parser = new PDDL12Parser();

        var problemTypes = new List<string>();
        var problemObjects = new List<PddlObject>();
        var problemInit = new List<PddlInit>();

        #region PROBLEM PARSE

        try
        {
            IReadOnlyList<IDefinition> list = parser.Parse(problemText);
            var p = (Problem) list[0];

            // OBJECTS & TYPES
            // using set to remove duplicates types
            HashSet<string> types = new HashSet<string>();
            foreach (IObject o in p.Objects)
            {
                // only if type doesn't exist in set add to the domain elements
                if (types.Add(o.Type.ToString()))
                {
                    problemTypes.Add(o.Type.ToString());
                }

                problemObjects.Add(new PddlObject(o.Value.ToString().ToLower(), o.Type.ToString()));
            }

            // INIT CODE
            foreach (var predicate in p.Initial)
            {
                /* Predicate Parameters*/
                var predicateParams = new List<string>();
                // Loop through parameters list
                foreach (var parameter in predicate.Parameters)
                {
                    // parameter in form name-type added to parameters list
                    predicateParams.Add(parameter.Value);
                }

                problemInit.Add(new PddlInit(predicate.Name.Value, predicateParams, !predicate.Positive));
            }
        }
        catch (PDDLSyntaxException pe)
        {
            Debug.LogError("Domain Syntax Error::" + pe.Message);
        }

        #endregion

        pTypes = problemTypes;
        objects = problemObjects;
        initPredicates = problemInit;
    }

    public static List<PlanAction> ParsePlan(JSONNode response)
    {
        List<PlanAction> actions = new List<PlanAction>();
        var planLength = int.Parse(response["result"]["length"]);
        for (var i = 0; i < planLength; i++)
        {
            string line = response["result"]["plan"][i]["name"];
            // Remove parentheses
            line = line.Remove(0, 1);
            line = line.Remove(line.Length - 1, 1);
            // Split spaces
            var split = line.Split(' ');
            // Action name is in first index
            var methodName = split[0];

            // Set action parameters
            var parameters = new List<string>();

            for (var j = 1; j < split.Length; j++)
            {
                parameters.Add(split[j]);
            }

            var action = new PlanAction(methodName, parameters) {parameters = parameters};
            actions.Add(action);
        }

        return actions;
    }
}