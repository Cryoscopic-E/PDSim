using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "SpawnToObject", menuName = "Predicates Behaviours/1 Attribute/Spawn Gameobject To Object")]
public class SpawnGameObjectToObject : OneAttributeCommand
{
    public GameObject objectToSpawn;
    public Alignment alignment;

    protected override IEnumerator Positive()
    {
        var holder = GameObject.Find("Custom Objects").transform;
        var clone = Instantiate(objectToSpawn, X.GetAlignmentPoint(alignment), Quaternion.identity,holder);
        yield return null;
    }
}