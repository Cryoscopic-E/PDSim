using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "Object To Object", menuName = "Predicates Behaviours/Object To Object")]
public class ObjectMoveToObject : PredicateCommand
{
    [SerializeField]
    public Alignment alignment;
    
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
        var y = attributes[1];
        yield return x.MoveToObjectAlignedTo(y, alignment);
        yield return new WaitForSeconds(0.4f);
    }

    protected override IEnumerator PostActivate()
    {
        yield return null;
    }
}
