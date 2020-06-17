using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "Object To Point", menuName = "Predicates Behaviours/Object to Point")]
public class ObjectMoveToPoint : PredicateCommand
{
    [SerializeField] public Vector3 target;


    protected override IEnumerator PreActivate()
    {
        yield return null;
    }

    protected override IEnumerator Activate()
    {
        var x = attributes[0];
        yield return x.Move(target);
        yield return new WaitForSeconds(0.4f);
    }

    protected override IEnumerator PostActivate()
    {
        yield return null;
    }
}
