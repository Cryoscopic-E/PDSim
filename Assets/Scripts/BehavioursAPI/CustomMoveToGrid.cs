using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "Object To GRid", menuName = "Predicates Behaviours/1 Attribute/Custom Move To Grid")]
public class CustomMoveToGrid : OneAttributeCommand
{
    protected override IEnumerator Positive()
    {
        var split = X.name.Split('-');
        var vx = int.Parse(split[1]);
        var vz = int.Parse(split[2]);
        yield return X.Move(new Vector3(vx, 0.0f, vz), true);
        yield return null;
    }
}
