using UnityEngine;
using System.Collections.Generic;
using PDDL.Parser;
using PDDL.Model.PDDL12;

public static class Parser
{
    public static DomainElements ParseDomain(string domainText)
    {
        PDDL12Parser parser = new PDDL12Parser();
        DomainElements elements = new DomainElements();
        // Domain
        try
        {
            IReadOnlyList<IDefinition> list = parser.Parse(domainText);
            var domain = (Domain)list[0];
            
            // TYPES
            foreach (IType type in domain.Types)
            {
                elements.types.Add(type.ToString());
            }
            
            // PREDICATES
            foreach (IAtomicFormulaSkeleton predicate in domain.Predicates)
            {
                List<PddlObject> objs = new List<PddlObject>();
                foreach (IVariableDefinition par in predicate.Parameters)
                {
                    objs.Add(new PddlObject(par.ToString(), par.Type.ToString()));
                }
                elements.predicates.Add(new PddlPredicate(predicate.Name.ToString(), objs));
            }
            
            // ACTIONS
            foreach (IAction action in domain.Actions)
            {
                List<PddlObject> objs = new List<PddlObject>();
                foreach (IVariableDefinition par in action.Parameters)
                {
                    objs.Add(new PddlObject(par.ToString(), par.Type.ToString()));
                }

                elements.actions.Add(new PddlAction(action.Functor.Value, objs));
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

    public static ProblemElements ParseProblem(string problem)
    {
        PDDL12Parser parser = new PDDL12Parser();
        ProblemElements elements = new ProblemElements();
        try
        {
            IReadOnlyList<IDefinition> list = parser.Parse(problem);
            var p = (Problem)list[0];

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
}
