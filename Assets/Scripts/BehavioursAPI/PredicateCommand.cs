using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PredicateCommand : ScriptableObject
{
    protected List<GenericObject> attributes;

    protected abstract IEnumerator PreActivate();
    protected abstract IEnumerator ActivatePositive();
    protected abstract IEnumerator ActivateNegative();
    protected abstract IEnumerator PostActivate();

    public IEnumerator Execute(bool negate)
    {
        yield return PreActivate();
        if (negate)
        {
            yield return ActivateNegative();
        }
        else
        {
            yield return ActivatePositive();
        }
        yield return PostActivate();
    }
    
    public void SetAttributes(List<GenericObject> attributes)
    {
        this.attributes = attributes;
    }
}
