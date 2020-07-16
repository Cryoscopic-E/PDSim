using System.Collections;
using UnityEngine;
[CreateAssetMenu(fileName = "ChangeColor", menuName = "Predicates Behaviours/Object Change Color")]
public class ChangeColorObject : PredicateCommand
{
    [SerializeField] public Color positiveColor;
    [SerializeField] public Color negativeColor;
    protected override IEnumerator PreActivate()
    {
        yield return null;
    }

    protected override IEnumerator ActivatePositive()
    {
        var x = attributes[0];
        x.ChangeColor(positiveColor);
        yield return null;
    }

    protected override IEnumerator ActivateNegative()
    {
        Debug.Log("NEgative color");
        var x = attributes[0];
        x.ChangeColor(negativeColor);
        yield return null;
    }

    protected override IEnumerator PostActivate()
    {
        yield return null;
    }
}
