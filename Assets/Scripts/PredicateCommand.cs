using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PredicateCommand : ScriptableObject
{
    protected List<GenericObject> attributes;

    protected abstract IEnumerator PreActivate();
    protected abstract IEnumerator Activate();
    protected abstract IEnumerator PostActivate();

    public IEnumerator Execute()
    {
        yield return PreActivate();
        yield return Activate();
        yield return PostActivate();
    }
    
    public void SetAttributes(List<GenericObject> attributes)
    {
        this.attributes = attributes;
    }
}
