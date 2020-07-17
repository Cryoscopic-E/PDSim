using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class CommandBase : ScriptableObject
{
    public bool waitBetweenActivations = false;
    public float timeBetweenActivations = 0.3f;
    protected virtual IEnumerator PreExecution()
    {
        yield return null;
    }

    protected virtual IEnumerator Positive()
    {
        yield return null;
    }

    protected virtual IEnumerator Negative()
    {
        yield return null;
    }

    protected virtual IEnumerator PostExecution()
    {
        yield return null;
    }

    public IEnumerator Execute(bool negate)
    {
        yield return PreExecution();
        if (negate)
            yield return Negative();
        else
            yield return Positive();
        yield return PostExecution();
        
        if (waitBetweenActivations)
            yield return new WaitForSeconds(timeBetweenActivations);
        else
            yield return null;
    }

    public abstract void Init(List<GenericObject> attributeList);
}

public abstract class OneAttributeCommand : CommandBase
{
    public readonly int AttributeCount = 1;
    protected GenericObject X;
    public override void Init(List<GenericObject> attributeList)
    {
        X = attributeList[0];
    }
}

public abstract class TwoAttributeCommand : CommandBase
{
    public readonly int AttributeCount = 2;
    protected GenericObject X;
    protected GenericObject Y;
    public override void Init(List<GenericObject> attributeList)
    {
        X = attributeList[0];
        Y = attributeList[1];
    }
}

public abstract class ThreeAttributeCommand : CommandBase
{
    public readonly int AttributeCount = 3;
    protected GenericObject X;
    protected GenericObject Y;
    protected GenericObject Z;
    public override void Init(List<GenericObject> attributeList)
    {
        X = attributeList[0];
        Y = attributeList[1];
        Z = attributeList[2];
    }
}
