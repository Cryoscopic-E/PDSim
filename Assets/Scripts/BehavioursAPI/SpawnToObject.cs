using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "SpawnToObject", menuName = "Predicates Behaviours/Spawn To Object")]
public class SpawnToObject : PredicateCommand
{
    public GameObject objectToSpawn;
    public Alignment alignment;
    protected override IEnumerator PreActivate()
    {
        yield return null;
    }

    protected override IEnumerator ActivatePositive()
    {
        var x = attributes[0];
        var holder = GameObject.Find("Custom Objects").transform;
        var clone = Instantiate(objectToSpawn, x.GetAlignmentPoint(alignment), Quaternion.identity,holder);
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