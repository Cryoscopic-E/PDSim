using System.Collections.Generic;
using UnityEngine;
public class SimulationSettings : ScriptableObject
{
    // ===============
    // DOMAIN SETTINGS
    // ===============
    
    // The domain file
    [SerializeField] public TextAsset domain;
    // Domain's list of actions
    [SerializeField] public List<PddlAction> actions;
    // Domain's list of predicates
    [SerializeField] public List<PddlPredicate> predicates;
    
    // =====================
    // UNITY REPRESENTATIONS
    // =====================
    
    // Predicate behaviour to call when an action is executed or during the init block
    [SerializeField] public PredicateCommand[] predicatesBehaviours;
    
    // ==============
    // PUBLIC METHODS
    // ==============
    
    public PredicateCommand GetPredicateBehaviour(string predicateName)
    {
        var index = predicates.FindIndex(a => a.name.Contains(predicateName));
        return predicatesBehaviours[index];
    }
    
    public List<PddlEffect> GetActionEffects(string actionName)
    {
        foreach (var a in actions)
        {
            if (a.name.ToLower() == actionName.ToLower())
            {
                return a.effects;
            }
        }
        return null;
    }
}                       