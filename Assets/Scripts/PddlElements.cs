using System.Collections.Generic;

[System.Serializable]
public class PddlElements
{
    public List<string> types;
    public List<PddlAction> actions;
    public List<PddlPredicate> predicates;
    public List<PddlObject> objects;

    public PddlElements()
    {
        this.types = new List<string>();
        this.actions = new List<PddlAction>();
        this.predicates = new List<PddlPredicate>();
        this.objects = new List<PddlObject>();
    }
}

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



