using System.Collections;
using UnityEngine;
[CreateAssetMenu(fileName = "ObjectFromToDirection", menuName = "Predicates Behaviours/Object From To Direction")]
public class ObjectFromToDirection : PredicateCommand
{
    protected override IEnumerator PreActivate()
    {
        yield return null;
    }

    protected override IEnumerator ActivatePositive()
    {
        var moveObject = attributes[0];
        var target = attributes[1];
        var direction = attributes[2];
        switch (direction.name)
        {
            case "dir-up":
                yield return moveObject.MoveToObjectAlignedTo(target, Alignment.Front);
                break;
            case "dir-down":
                yield return moveObject.MoveToObjectAlignedTo(target, Alignment.Back);
                break;
            case "dir-left":
                yield return moveObject.MoveToObjectAlignedTo(target, Alignment.Left);
                break;
            case "dir-right":
                yield return moveObject.MoveToObjectAlignedTo(target, Alignment.Right);
                break;
            default:
                yield return null;
                break;
        }
        yield return new WaitForSeconds(timeBetweenActivations);
    }

    protected override IEnumerator ActivateNegative()
    {
        yield return null;
    }

    protected override IEnumerator PostActivate()
    {
        yield return null;
    }
}
