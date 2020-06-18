using System;
using System.Collections;
using UnityEngine;

public enum Alignment { NONE, TOP, BOTTOM, LEFT, RIGHT, FRONT, BACK };


public class GenericObject : MonoBehaviour
{
    

    [Header("Transforms")]
    public Transform top;
    public Transform bottom;
    public Transform left;
    public Transform right;
    public Transform front;
    public Transform back;
    [Space] 
    [Header("States HUD")] 
    public GameObject stateCanvas;
    
    [Space] 
    [Header("Debug")] 
    public bool showDebugGizmos = true;
    
    
    private void OnMouseEnter()
    {
        stateCanvas.SetActive(true);
    }

    private void OnMouseExit()
    {
        stateCanvas.SetActive(false);
    }

    // ANIMATIONS

    public IEnumerator Move(Vector3 end)
    {
        StopAllCoroutines();
        yield return StartCoroutine(MoveTo(transform.position, end));
    }

    

    public IEnumerator MoveToObjectAlignedTo(GenericObject obj, Alignment align = Alignment.NONE)
    {
        StopAllCoroutines();
        switch (align)
        {
            case Alignment.TOP:
                yield return StartCoroutine(MoveTo(bottom.position, obj.top.position));
                break;
            case Alignment.BOTTOM:
                yield return StartCoroutine(MoveTo(top.position, obj.bottom.position));
                break;
            case Alignment.LEFT:
                yield return StartCoroutine(MoveTo(right.position, obj.left.position));
                break;
            case Alignment.RIGHT:
                yield return StartCoroutine(MoveTo(left.position, obj.right.position));
                break;
            case Alignment.FRONT:
                yield return StartCoroutine(MoveTo(back.position, obj.front.position));
                break;
            case Alignment.BACK:
                yield return StartCoroutine(MoveTo(front.position, obj.back.position));
                break;
            default:
                yield return StartCoroutine(MoveTo(transform.position, obj.transform.position));
                break;
        }

        yield return null;
    }

    

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

    void OnDrawGizmos()
    {
        if (showDebugGizmos)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(transform.position, Vector3.one);
            Gizmos.color = new Color(1.0f, 0.0f, 0.0f, 0.5f);
            Gizmos.DrawCube(transform.position, Vector3.one);
            Gizmos.color = Color.gray;
            Gizmos.DrawSphere(top.position, 0.1f);
            Gizmos.DrawSphere(bottom.position, 0.1f);
            Gizmos.DrawSphere(left.position, 0.1f);
            Gizmos.DrawSphere(right.position, 0.1f);
            Gizmos.DrawSphere(back.position, 0.1f);
            Gizmos.DrawSphere(front.position, 0.1f);
        }
    }
}
