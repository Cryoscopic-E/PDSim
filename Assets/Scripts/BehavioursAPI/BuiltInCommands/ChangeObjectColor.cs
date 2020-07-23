using System.Collections;
using UnityEngine;
[CreateAssetMenu(fileName = "ChangeColor", menuName = "Predicates Behaviours/1 Attribute/Change Object Color")]
public class ChangeObjectColor : OneAttributeCommand
{
    [SerializeField] public Color positiveColor;
    [SerializeField] public Color negativeColor;
    
    protected override IEnumerator Positive()
    {
        X.ChangeColor(positiveColor);
        yield return null;
    }
    protected override IEnumerator Negative()
    {
        X.ChangeColor(negativeColor);
        yield return null;
    }
}
