using System.Collections;
using UnityEngine;
[CreateAssetMenu(fileName = "ChangeColor", menuName = "Predicates Behaviours/Object Change Color")]
public class ChangeColorObject : PredicateCommand
{
    [SerializeField] public Color newColor;
    protected override IEnumerator PreActivate()
    {
        yield return null;
    }

    protected override IEnumerator ActivatePositive()
    {
        var x = attributes[0];
        x.ChangeColor(newColor);
        yield return null;
    }

    protected override IEnumerator ActivateNegative()
    {
        yield return null;
    }

    protected override IEnumerator PostActivate()
    {
        yield return null;
    }
}
