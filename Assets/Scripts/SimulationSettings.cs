using System;
using System.Collections.Generic;
using UnityEditor.VersionControl;
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
    [SerializeField] public List<PddlDomainPredicate> predicates;
    // Domain's list of types
    [SerializeField] public List<string> typesToDefine;
    [SerializeField] public List<int> typesDefined; //maps indexes to typesToDefine
    [SerializeField] public List<PddlType> types;
    
    
    // name of simulation
    [SerializeField] public string simulationName;
    

    // Generic object models for types
    [SerializeField] public List<GameObject> typesGameObject;
    
    
    // ==============
    // PUBLIC METHODS
    // ==============
    
    public List<PredicateCommandSettings> GetPredicateCommandSettings(string predicateName)
    {
        var index = predicates.FindIndex(a => a.predicateName.Contains(predicateName));
        return predicates[index].predicateCommandSettings;
    }
    
    public List<PddlEffectPredicate> GetActionEffects(string actionName)
    {
        foreach (var action in actions)
        {
            if (string.Equals(action.actionName, actionName, StringComparison.CurrentCultureIgnoreCase))
            {
                return action.effects;
            }
        }
        return null;
    }
    
    public GameObject GetPrefabWithType(PddlType type)
    {
        var index = typesToDefine.FindIndex(a => a.Contains(type.typeName));

        var definedIndex = typesDefined.FindIndex(t => t == index);
        return index != -1 ? typesGameObject[definedIndex] : null;
    }
}                       