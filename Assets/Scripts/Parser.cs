using UnityEngine;
using System.Collections.Generic;
using PDDL.Parser;
using PDDL.Model.PDDL12;

public static class Parser
{
    public static PddlElements ParseDomainAndProblem(string domain, string problem)
    {
        PDDL12Parser parser = new PDDL12Parser();
        Domain d;
        Problem p;
        PddlElements elements = new PddlElements();
        // Domain
        try
        {
            IReadOnlyList<IDefinition> list = parser.Parse(domain);
            d = (Domain)list[0];
            foreach (IType type in d.Types)
            {
                elements.types.Add(type.ToString());
            }
            foreach (IAction action in d.Actions)
            {
                List<PddlObject> objs = new List<PddlObject>();
                foreach (IVariableDefinition par in action.Parameters)
                {
                    objs.Add(new PddlObject(par.ToString(), par.Type.ToString()));
                }

                elements.actions.Add(new PddlAction(action.ToString(), objs));
            }
            foreach (IAtomicFormulaSkeleton predicate in d.Predicates)
            {
                List<PddlObject> objs = new List<PddlObject>();
                foreach (IVariableDefinition par in predicate.Parameters)
                {
                    objs.Add(new PddlObject(par.ToString(), par.Type.ToString()));
                }
                elements.predicates.Add(new PddlPredicate(predicate.Name.ToString(), objs));
            }
        }
        catch (PDDLSyntaxException pe)
        {
            Debug.LogError("Domain Syntax Error::" + pe.Message);
        }



        // Problem
        try
        {
            IReadOnlyList<IDefinition> list = parser.Parse(problem);
            p = (Problem)list[0];

            foreach (IObject o in p.Objects)
            {
                
                elements.objects.Add(new PddlObject(o.ToString(), o.Type.ToString()));
            }
        }
        catch (PDDLSyntaxException pe)
        {
            Debug.LogError("Domain Syntax Error::" + pe.Message);
        }
        return elements;
    }

    public static List<PlanAction> ParsePlan(string response)
    {
        List<PlanAction> actions = new List<PlanAction>();

        return actions;
    }
}
