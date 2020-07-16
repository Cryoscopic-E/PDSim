using System;
using System.Collections.Generic;
using System.Data;
using UnityEngine;

[Serializable]
public class PddlType : IComparable<PddlType>
{
    public  string typeName;
    public  string parentTypeName;

    public PddlType(string typeName, string parentType = "")
    {
        this.typeName = typeName;
        parentTypeName = parentType;
    }

    public int CompareTo(PddlType other)
    {
        if (ReferenceEquals(this, other)) return 0;
        if (ReferenceEquals(null, other)) return 1;
        var typeNameComparison = string.Compare(typeName, other.typeName, StringComparison.OrdinalIgnoreCase);
        return typeNameComparison != 0 ? typeNameComparison : string.Compare(parentTypeName, other.parentTypeName, StringComparison.OrdinalIgnoreCase);
    }

    public override string ToString()
    {
        return typeName.ToLower();
    }
}

[Serializable]
public class PddlObject
{
    public  string objectName;
    public PddlType objectType;

    public PddlObject(string objectName, PddlType objectType)
    {
        this.objectName = objectName;
        this.objectType = objectType;
    }
    public override bool Equals(object obj)
    {
        var otherObject = (PddlObject) obj;
        return otherObject != null && string.Compare(objectName.ToLower(), otherObject.objectName.ToLower(), StringComparison.Ordinal) == 0;
    }
    public override int GetHashCode()
    {
        return (objectName != null ? objectName.GetHashCode() : 0);
    }

    public override string ToString()
    {
        return objectName.ToLower();
    }
}

[Serializable]
public class PddlEffectPredicate 
{
    public  string predicateName;
    public  bool isNegated;
    public List<int> attributesIndexes;
    public PddlEffectPredicate(string predicateName, bool isNegated, List<int> attributesIndexes)
    {
        this.predicateName = predicateName;
        this.isNegated = isNegated;
        this.attributesIndexes = attributesIndexes;
    }
}


[Serializable]
public class PddlInitPredicate
{
    public  string predicateName;
    public  bool isNegated;
    public List<string> objectParameters;
    public PddlInitPredicate(string predicateName, List<string> objectParameters, bool isNegated)
    {
        this.predicateName = predicateName;
        this.isNegated = isNegated;
        this.objectParameters = objectParameters;
    }
}

[Serializable]
public class PddlDomainPredicate
{
    public  string predicateName;
    public  bool isNegated;
    public List<PddlObject> parameters;
    public  string parametersDescription;
    public List<string> parametersTypes;
    
    
    public List<PredicateCommandSettings> predicateCommandSettings;
    public PddlDomainPredicate(string predicateName, List<PddlObject> parameters, bool isNegated = false)
    {
        this.predicateName = predicateName;
        this.isNegated = isNegated;
        this.parameters = parameters;
        foreach (var parameter in parameters)
        {
            parametersDescription += parameter.objectName + " - " + parameter.objectType.typeName + "   ";
        }
        parametersTypes = new List<string>();
        predicateCommandSettings = new List<PredicateCommandSettings>();
    }
}


[Serializable]
public class PddlAction
{
    public  string actionName;
    public List<PddlObject> parameters;
    public List<PddlEffectPredicate> effects;
    public PddlAction(string actionName, List<PddlObject> parameters, List<PddlEffectPredicate> effects)
    {
        this.actionName = actionName;
        this.parameters = parameters;
        this.effects = effects;
    }

    public override bool Equals(object obj)
    {
        var otherAction = (PddlAction) obj;
        return otherAction != null && string.Compare(actionName.ToLower(), otherAction.actionName.ToLower(),
            StringComparison.Ordinal) == 0;
    }

    public override int GetHashCode()
    {
        return (actionName != null ? actionName.GetHashCode() : 0);
    }

    public override string ToString()
    {
        return actionName.ToLower();
    }
}

[Serializable]
public struct PlanAction
{
    public string actionName;
    public List<string> parameters;
    

    public PlanAction(string actionName, List<string> parameters)
    {
        this.actionName = actionName;
        this.parameters = parameters;
    }

    public string Parameters()
    {
        return string.Join(", ", parameters);
    }
}