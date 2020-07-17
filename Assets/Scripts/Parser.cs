using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using PDDL.Parser;
using PDDL.Model.PDDL12;
using PDDL.Model.PDDL12.Effects;
using PDDL.Model.PDDL12.Types;
using SimpleJSON;

public static class Parser
{
    public static void ParseDomain(string domainText,out List<PddlType>types, out List<PddlDomainPredicate> predicates, out List<PddlAction> actions)
    {
        // Instancing the PDDL parser 
        var parser = new PDDL12Parser();

        var domainPredicates = new List<PddlDomainPredicate>();
        var domainActions = new List<PddlAction>();
        var domainTypes = new List<PddlType>();
        var tempTypesDictionary = new Dictionary<string, PddlType>();
        
        # region DOMAIN PARSE

        try
        {
            // Get definitions list
            var list = parser.Parse(domainText);
            // Get Domain
            var domain = (Domain) list[0];
            
            
            /* CREATE TYPES */
            var typesList = domain.Types;
            foreach (var t in typesList)
            {
                var type = t as CustomType;
                if (type == null) continue;

                if (type.Parent is CustomType parent)
                {
                    var parsedTypeParent = parent.Name.Value;
                    var parsedType = new PddlType(type.Name.Value, parsedTypeParent);
                    domainTypes.Add(parsedType);
                    tempTypesDictionary.Add(type.Name.Value, parsedType);
                    continue;
                }

                var rootType = new PddlType(type.Name.Value);
                domainTypes.Add(rootType);
                tempTypesDictionary.Add(type.Name.Value, rootType);
            }
            
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
                    predicateParams.Add(new PddlObject(parameter.Value.Name.Value, tempTypesDictionary[parameter.Type.ToString()]));
                }

                // add predicate to list
                domainPredicates.Add(new PddlDomainPredicate(predicate.Name.Value, predicateParams));
            }
            
            //create list of predicate types for each predicate parameter to account the type inheritance
            foreach (var predicate in domainPredicates)
            {
                foreach (var parameter in predicate.parameters)
                {
                    // add the parameter type
                    predicate.parametersTypes.Add(parameter.objectType.typeName);
                    // check children
                    foreach (var type in domainTypes)
                    {
                        
                        if (type.parentTypeName != string.Empty && 
                            type.parentTypeName == parameter.objectType.typeName &&
                            !predicate.parametersTypes.Contains(type.typeName))
                        {
                            predicate.parametersTypes.Add(type.typeName);
                        }
                    }
                }
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
                    actionParams.Add(new PddlObject(parameter.Value.ToString(), tempTypesDictionary[parameter.Type.ToString()]));
                }

                /* Action's Effects */
                var effects = new List<PddlEffectPredicate>();
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
                            indexes.Add(actionParams.FindIndex(a => a.objectName.Equals(parameter.ToString())));
                        }
                        effects.Add(new PddlEffectPredicate(negatedPredicate.Effects.Name.Value, true, indexes));
                    }
                    else
                    {
                        var regularPredicate = (RegularEffect) predicate;
                        var indexes = new List<int>();
                        foreach (var parameter in regularPredicate.Effects.Parameters)
                        {
                            indexes.Add(actionParams.FindIndex(a => a.objectName.Equals(parameter.ToString())));
                        }
                        effects.Add(new PddlEffectPredicate(regularPredicate.Effects.Name.Value, false, indexes));
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
        types = domainTypes;
    }


    public static void ParseProblem(string problemText, out List<string> pTypes, out List<PddlObject> objects,
        out List<PddlInitPredicate> initPredicates)
    {
        // Instancing the PDDL parser 
        var parser = new PDDL12Parser();

        var problemTypes = new List<string>();
        var problemObjects = new List<PddlObject>();
        var problemInit = new List<PddlInitPredicate>();

        #region PROBLEM PARSE

        try
        {
            IReadOnlyList<IDefinition> list = parser.Parse(problemText);
            var problem = (Problem) list[0];

            // OBJECTS & TYPES
            // using set to remove duplicates types
            var types = new HashSet<string>();
            foreach (var _object in problem.Objects)
            {
                // only if type doesn't exist in set add to the domain elements
                if (types.Add(_object.Type.ToString()))
                {
                    problemTypes.Add(_object.Type.ToString());
                }

                problemObjects.Add(new PddlObject(_object.Value.ToString().ToLower(), new PddlType(_object.Type.ToString())));
            }

            // INIT CODE
            foreach (var predicate in problem.Initial)
            {
                /* Predicate Parameters*/
                var predicateParams = new List<string>();
                // Loop through parameters list
                foreach (var parameter in predicate.Parameters)
                {
                    // parameter in form name-type added to parameters list
                    predicateParams.Add(parameter.Value);
                }

                problemInit.Add(new PddlInitPredicate(predicate.Name.Value, predicateParams, !predicate.Positive));
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
        var actions = new List<PlanAction>();
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