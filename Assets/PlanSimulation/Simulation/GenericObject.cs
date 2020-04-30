using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Alignment { NONE, TOP, BOTTOM, LEFT, RIGHT, FRONT, BACK };

public class GenericObject : MonoBehaviour
{
    public Transform top;
    public Transform bottom;
    public Transform left;
    public Transform right;
    public Transform front;
    public Transform back;

    public MeshFilter meshFilter;
    public MeshRenderer meshRenderer;

    public void MoveToObjectAlignedTo(GenericObject obj, Alignment align = Alignment.NONE)
    {
        StopAllCoroutines();
        switch (align)
        {
            case Alignment.TOP:
                StartCoroutine(MoveTo(bottom.position, obj.top.position));
                break;
            case Alignment.BOTTOM:
                StartCoroutine(MoveTo(top.position, obj.bottom.position));
                break;
            case Alignment.LEFT:
                StartCoroutine(MoveTo(right.position, obj.left.position));
                break;
            case Alignment.RIGHT:
                StartCoroutine(MoveTo(left.position, obj.right.position));
                break;
            case Alignment.FRONT:
                StartCoroutine(MoveTo(back.position, obj.front.position));
                break;
            case Alignment.BACK:
                StartCoroutine(MoveTo(front.position, obj.back.position));
                break;
            default:
                StartCoroutine(MoveTo(transform.position, obj.transform.position));
                break;
        }
    }

    // ANIMATIONS

    IEnumerator MoveTo(Vector3 start, Vector3 end)
    {
        Vector3 pointOffset = transform.position - start;
        Vector3 adjustEnd = end + pointOffset;

        Vector3 offSet = transform.position - adjustEnd;
        float sqrOffSet = offSet.sqrMagnitude;
        while (sqrOffSet > 0.05f)
        {
            transform.position = Vector3.Lerp(transform.position, adjustEnd, 0.1f);
            offSet = transform.position - adjustEnd;
            sqrOffSet = offSet.sqrMagnitude;
            yield return new WaitForSeconds(0.1f);
        }
        transform.position = adjustEnd;
        yield return null;
    }
}
