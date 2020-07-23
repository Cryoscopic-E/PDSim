using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "Object To Object", menuName = "Predicates Behaviours/2 Attributes/Object To Object")]
public class ObjectToObject : TwoAttributeCommand
{
    [SerializeField]
    public Alignment alignment;
    [SerializeField] public bool attachDetachToTarget;
    [SerializeField] public bool randomRadius;
    [SerializeField] public float radius;
    [SerializeField] public bool matchHeightFirst;
    
    protected override IEnumerator PreExecution()
    {
        if (matchHeightFirst)
        {
            var newTarget = new Vector3(X.transform.position.x, Y.GetAlignmentPoint(Alignment.Top).y, X.transform.position.z);
            yield return X.Move(newTarget);
        }
    }

    protected override IEnumerator Negative()
    {
        if (attachDetachToTarget)
        {
            Y.Detach(X.gameObject);
        }

        yield return null;
    }

    protected override IEnumerator Positive()
    {
        if (randomRadius)
        {
            Vector3 offset = new Vector3();
            offset.x += Random.Range(-radius,radius);
            offset.z += Random.Range(-radius, radius);
            yield return X.MoveToObjectAlignedTo(Y, alignment, offset);
        }
        else
        {
            yield return X.MoveToObjectAlignedTo(Y, alignment);
        }

        if (attachDetachToTarget)
        {
            Y.Attach(X.gameObject);
        }

        yield return null;
    }

}
