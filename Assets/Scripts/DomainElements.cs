using System.Collections.Generic;

[System.Serializable]
public class DomainElements
{
    public List<string> types;
    public List<PddlAction> actions;
    public List<PddlPredicate> predicates;

    public DomainElements()
    {
        this.types = new List<string>();
        this.actions = new List<PddlAction>();
        this.predicates = new List<PddlPredicate>();
    }
}