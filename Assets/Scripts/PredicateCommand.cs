using System.Collections.Generic;
using UnityEngine;

public abstract class PredicateCommand : ScriptableObject
{
    protected List<GenericObject> attributes;

    protected abstract void PreActivate();
    protected abstract void Activate();
    protected abstract void PostActivate();

    public void Execute()
    {
        PreActivate();
        Activate();
        PostActivate();
    }
    
    public void SetAttributes(List<GenericObject> attributes)
    {
        this.attributes = attributes;
    }
}
