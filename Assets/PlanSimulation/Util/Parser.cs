using UnityEngine;
using System.Collections.Generic;
using PDDL.Parser;
using PDDL.Model.PDDL12;

public class Parser
{
    private static Parser instance;
    private bool parsed;
    public List<string> types;
    public List<string> objects;
    public List<string> predicates;
    public List<string> actions;

    protected Parser()
    {
        parsed = false;
        types = new List<string>();
        objects = new List<string>();
        predicates = new List<string>();
        actions = new List<string>();
    }

    public static Parser Instance()
    {
        if(instance == null)
        {
            instance = new Parser();
        }
        return instance;
    }

    public bool IsParsed()
    {
        return parsed;
    }

    public void Reset()
    {
        parsed = false;
        types.Clear();
        objects.Clear();
        predicates.Clear();
        actions.Clear();
    }

    public void ParseDomainProblem(string domain, string problem)
    {
        PDDL12Parser parser = new PDDL12Parser();
        Domain d;
        Problem p;
        // Domain
        try
        {
            IReadOnlyList<IDefinition> list = parser.Parse(domain);
            d = (Domain)list[0];
            foreach (IType type in d.Types)
            {
                types.Add(type.ToString());
            }
            foreach (IAction action in d.Actions)
            {
                actions.Add(action.ToString());
            }
            foreach (IAtomicFormulaSkeleton predicate in d.Predicates)
            {
                predicates.Add(predicate.ToString());
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
                objects.Add(o.ToString());
            }
        }
        catch (PDDLSyntaxException pe)
        {
            Debug.LogError("Domain Syntax Error::" + pe.Message);
        }
        parsed = true;
    }
}
