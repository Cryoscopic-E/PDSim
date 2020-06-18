using PDDL.Model.PDDL12;
using System.Collections.Generic;
using System.Security.Permissions;

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
public struct PddlInit
{
    public string predicateName;
    public List<string> parameters;
    public bool negate;

    public PddlInit(string name, List<string> p, bool neg)
    {
        predicateName = name;
        parameters = p;
        negate = neg;
    }
}

[System.Serializable]
public struct PddlPredicate
{
    public string name;
    public List<PddlObject> parameters;
    public bool negate;
    public PddlPredicate(string n, List<PddlObject> pars, bool nt = false)
    {
        name = n;
        parameters = pars;
        negate = nt;
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