using UnityEngine;

[CreateAssetMenu(fileName = "Object To Point", menuName = "Predicates Behaviours/Object to Point")]
public class ObjectMoveToPoint : PredicateCommand
{
    [SerializeField] public Vector3 target;


    protected override void PreActivate()
    {
    }

    protected override void Activate()
    {
        var x = attributes[0];
        x.Move(target);
    }

    protected override void PostActivate()
    {
    }
}
