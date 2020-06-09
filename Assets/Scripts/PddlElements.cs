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
public struct PddlPredicate
{
    public bool predicateState;
    public string name;
    public List<PddlObject> parameters;

    public PddlPredicate(string n, List<PddlObject> pars, bool s = true)
    {
        predicateState = s;
        name = n;
        parameters = pars;
    }
}

[System.Serializable]
public struct PddlAction
{
    public string name;
    public List<PddlObject> parameters;

    public PddlAction(string n, List<PddlObject> pars)
    {
        name = n;
        parameters = pars;
    }
}