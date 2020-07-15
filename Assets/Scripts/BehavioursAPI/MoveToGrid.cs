using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "Object To GRid", menuName = "Predicates Behaviours/Object To Grid")]
public class MoveToGrid : PredicateCommand
{
    protected override IEnumerator PreActivate()
    {
        yield return null;
    }

    protected override IEnumerator ActivateNegative()
    {
        yield return null;
    }

    protected override IEnumerator ActivatePositive()
    {
        var x = attributes[0];
        var split = x.name.Split('-');
        var vx = int.Parse(split[1]);
        var vz = int.Parse(split[2]);
        yield return x.Move(new Vector3(vx, 0.0f, vz), true);
        yield return null;
    }

    protected override IEnumerator PostActivate()
    {
        yield return null;
    }
}
