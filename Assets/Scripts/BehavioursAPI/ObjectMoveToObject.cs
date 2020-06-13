using UnityEngine;

[CreateAssetMenu(fileName = "Object To Object", menuName = "Predicates Behaviours/Object To Object")]
public class ObjectMoveToObject : PredicateCommand
{
    [SerializeField]
    public Alignment alignment;
    
    protected override void PreActivate()
    {
    }

    protected override void Activate()
    {
        var x = attributes[0];
        var y = attributes[1];
        x.MoveToObjectAlignedTo(y, alignment);
    }

    protected override void PostActivate()
    {
    }
}
