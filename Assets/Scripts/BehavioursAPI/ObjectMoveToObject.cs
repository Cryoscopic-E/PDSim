using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "Object To Object", menuName = "Predicates Behaviours/Object To Object")]
public class ObjectMoveToObject : PredicateCommand
{
    [SerializeField]
    public Alignment alignment;
    
    [SerializeField] public bool randomRadius;
    [SerializeField] public float radius;
    [SerializeField] public bool matchHeightFirst;
    
    protected override IEnumerator PreActivate()
    {
        if (matchHeightFirst)
        {
            var x = attributes[0];
            var y = attributes[1];
            var newTarget = new Vector3(x.transform.position.x, y.GetAlignmentPoint(Alignment.Top).y, x.transform.position.z);
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
        var y = attributes[1];
        if (randomRadius)
        {
            Vector3 offset = new Vector3();
            offset.x += Random.Range(-radius,radius);
            offset.z += Random.Range(-radius, radius);
            yield return x.MoveToObjectAlignedTo(y, alignment, offset);
            yield return new WaitForSeconds(timeBetweenActivations);
        }
        else
        {
            yield return x.MoveToObjectAlignedTo(y, alignment);
            yield return new WaitForSeconds(timeBetweenActivations);
        }
        
    }

    protected override IEnumerator PostActivate()
    {
        yield return null;
    }
}
