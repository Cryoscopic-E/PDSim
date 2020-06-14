using PDDL.Model.PDDL12;
using System.Collections.Generic;

[System.Serializable]
public struct PddlObject
{
    public string name;
    public string type;

    public PddlObject(string n, string t)
    {
        name = n;
        type = t;
    }
}

[System.Serializable]
public struct PddlEffect
{
    public string predicateName;
    public bool negate;

    public PddlEffect(string name, bool neg)
    {
        predicateName = name;
        negate = neg;
    }
}

[System.Serializable]
public struct PddlPredicate
{
    public string name;
    public List<PddlObject> parameters;

    public PddlPredicate(string n, List<PddlObject> pars)
    {
        name = n;
        parameters = pars;
    }
}




[System.Serializable]
public struct PddlAction
{
    public string name;
    public List<PddlObject> parameters;
    public List<PddlEffect> effects;
    public PddlAction(string n, List<PddlObject> pars, List<PddlEffect> effs)
    {
        name = n;
        parameters = pars;
        effects = effs;
    }
}