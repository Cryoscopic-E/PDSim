using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Plan
{
    [SerializeField]
    public List<PlanAction> actions;

    public Plan()
    {
        actions = new List<PlanAction>();
    }
    
    public Plan(List<PlanAction> actions)
    {
        this.actions = actions;
    }
}
