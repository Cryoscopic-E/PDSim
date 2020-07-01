using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "Object To Point", menuName = "Predicates Behaviours/Object to Point")]
public class ObjectMoveToPoint : PredicateCommand
{
    [SerializeField] public Vector3 target;
    [SerializeField] public bool randomRadius;
    [SerializeField] public float radius;

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
        var newTarget = target;
        if (randomRadius)
        {
            newTarget.x += Random.Range(-radius,radius);
            newTarget.z += Random.Range(-radius, radius);
            yield return x.Move(newTarget);
        }
        else
        {
            yield return x.Move(target);
        }
        yield return new WaitForSeconds(0.4f);
    }

    protected override IEnumerator PostActivate()
    {
        yield return null;
    }
}
