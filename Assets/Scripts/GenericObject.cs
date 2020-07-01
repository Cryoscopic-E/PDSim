using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public enum Alignment { NONE, TOP, BOTTOM, LEFT, RIGHT, FRONT, BACK };


public class GenericObject : MonoBehaviour
{
    private Transform top;
    private Transform bottom;
    private Transform left;
    private Transform right;
    private Transform front;
    private Transform back;
    
    private GameObject stateCanvas;
    private Text nameText;
    private Text statesText;
    private Dictionary<string,bool> objectStates;
    
    
    [Header("Object Options")]
    
    public bool randomColor = false;

    public bool usePathPlanning = false;
    
    [Space] 
    [Header("Debug")] 
    public bool showDebugGizmos = true;

    private void Awake()
    {
        // Get internal references
        var transforms = transform.Find("Transforms");
        top = transforms.Find("Top");
        bottom = transforms.Find("Bottom");
        left = transforms.Find("Left");
        right = transforms.Find("Right");
        front = transforms.Find("Front");
        back = transforms.Find("Back");
        stateCanvas = transform.Find("State Canvas").gameObject;
        
        // set color if random
        if (randomColor)
        {
            var modelRenderer = transform.Find("3DModel").GetComponentInChildren<Renderer>();
            modelRenderer.material.color = GenerateRandomColor();
        }
        
        objectStates = new Dictionary<string, bool>();

        var texts = stateCanvas.GetComponentsInChildren<Text>();
        
        nameText = texts[0];
        statesText = texts[1];
        
        nameText.text = "Object "  + "'" +gameObject.name +"'";
        statesText.text = "";
    }

    private Color GenerateRandomColor()
    {
        return new Color(
            Random.Range(0f,1f),
            Random.Range(0f,1f),
            Random.Range(0f,1f)
            );
    }

    public void SetState(string name, bool negate)
    {
        objectStates[name] = negate;
        UpdateStateCanvas();
    }

    private void UpdateStateCanvas()
    {
        string message = "";
        foreach (var key in objectStates.Keys)
        {
            message += key + " " + !objectStates[key] + "\n";
        }

        statesText.text = message;
    }
    
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
