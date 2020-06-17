using UnityEngine;
using System.Collections.Generic;
using PDDL.Parser;
using PDDL.Model.PDDL12;
using PDDL.Model.PDDL12.Effects;

public static class Parser
{
    public static DomainElements ParseDomain(string domainText)
    {
        // Create new domain elements OOP representation
        DomainElements elements = new DomainElements();
        // Instancing the PDDL parser 
        PDDL12Parser parser = new PDDL12Parser();

        /* DOMAIN PARSING */
        try
        {
            // Get definitions list
            var list = parser.Parse(domainText);
            // Get Domain
            var domain = (Domain) list[0];

            /* CREATE PDDL TYPES */
            foreach (var type in domain.Types)
            {
                // add type name to types list
                elements.types.Add(type.ToString());
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
                    predicateParams.Add(new PddlObject(parameter.ToString(), parameter.Type.ToString()));
                }

                // add predicate to list
                elements.predicates.Add(new PddlPredicate(predicate.Name.ToString(), predicateParams));
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
                    actionParams.Add(new PddlObject(parameter.ToString(), parameter.Type.ToString()));
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
                        effects.Add(new PddlEffect(negatedPredicate.Effects.Name.Value, true));
                    }
                    else
                    {
                        var regularPredicate = (RegularEffect) predicate;
                        effects.Add(new PddlEffect(regularPredicate.Effects.Name.Value, false));
                    }
                }
                // add action to list
                elements.actions.Add(new PddlAction(action.Functor.Value, actionParams,effects));
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

    // public static ProblemElements ParseProblem(string problem)
    // {
    //     ProblemElements elements = new ProblemElements();
    //     PDDL12Parser parser = new PDDL12Parser();
    //     try
    //     {
    //         IReadOnlyList<IDefinition> list = parser.Parse(problem);
    //         var p = (Problem) list[0];
    //         // OBJECTS
    //         foreach (IObject o in p.Objects)
    //         {
    //             elements.objects.Add(new PddlObject(o.ToString(), o.Type.ToString()));
    //         }
    //
    //         // INIT CODE
    //         foreach (var predicate in p.Initial)
    //         {
    //         }
    //     }
    //     catch (PDDLSyntaxException pe)
    //     {
    //         Debug.LogError("Domain Syntax Error::" + pe.Message);
    //     }
    //
    //     return elements;
    // }
}