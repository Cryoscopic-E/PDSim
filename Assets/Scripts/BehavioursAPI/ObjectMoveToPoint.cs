using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "Object To Point", menuName = "Predicates Behaviours/Object to Point")]
public class ObjectMoveToPoint : PredicateCommand
{
    [SerializeField] public Vector3 target;
    [SerializeField] public bool randomRadius;
    [SerializeField] public float radius;
    [SerializeField] public bool matchHeightFirst;
    protected override IEnumerator PreActivate()
    {
       
        if (matchHeightFirst)
        {
            var x = attributes[0];
            var newTarget = new Vector3(x.transform.position.x, target.y, x.transform.position.z);
            yield return x.Move(newTarget);
        }
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
        yield return new WaitForSeconds(timeBetweenActivations);
    }

    protected override IEnumerator PostActivate()
    {
        yield return null;
    }
}
