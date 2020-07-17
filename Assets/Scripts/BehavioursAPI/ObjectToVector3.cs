using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "Object To Point", menuName = "Predicates Behaviours/1 Attribute/Object to Point")]
public class ObjectToVector3 : OneAttributeCommand
{
    [SerializeField] public Vector3 target;
    [SerializeField] public bool randomRadius;
    [SerializeField] public float radius;
    [SerializeField] public bool matchHeightFirst;
    protected override IEnumerator PreExecution()
    {
       
        if (matchHeightFirst)
        {
            var position = X.transform.position;
            var newTarget = new Vector3(position.x, target.y, position.z);
            yield return X.Move(newTarget);
        }
        yield return null;
    }
    protected override IEnumerator Positive()
    {
        var newTarget = target;
        if (randomRadius)
        {
            newTarget.x += Random.Range(-radius,radius);
            newTarget.z += Random.Range(-radius, radius);
            yield return X.Move(newTarget);
        }
        else
        {
            yield return X.Move(target);
        }
        yield return new WaitForSeconds(timeBetweenActivations);
    }
}
