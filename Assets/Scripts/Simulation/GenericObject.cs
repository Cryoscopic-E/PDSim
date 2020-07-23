using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public enum Alignment { None, Top, Bottom, Left, Right, Front, Back };


public class GenericObject : MonoBehaviour
{
    // Offsets
    private Transform _top;
    private Transform _bottom;
    private Transform _left;
    private Transform _right;
    private Transform _front;
    private Transform _back;
    
    // UI
    private GameObject _stateCanvas;
    private Text _nameText;
    private Text _statesText;
    private Dictionary<string,string> _objectStatesContent;
    private Dictionary<string,bool> _objectStates;
    private Renderer _modelRenderer;

    private List<GameObject> _attachedObjects;
    
    [Header("Object Options")]
    public bool randomColor;
    public bool usePathPlanning;
    private NavMeshAgent _navMeshAgent;
    
    [Space] 
    
    [Header("Debug")] 
    public bool showDebugGizmos = true;

    private void Awake()
    {
        // Get internal references
        var transforms = transform.Find("Transforms");
        _top = transforms.Find("Top");
        _bottom = transforms.Find("Bottom");
        _left = transforms.Find("Left");
        _right = transforms.Find("Right");
        _front = transforms.Find("Front");
        _back = transforms.Find("Back");
        _stateCanvas = transform.Find("State Canvas").gameObject;
        _modelRenderer = transform.Find("3DModel").GetComponentInChildren<Renderer>();
        // set color if random
        if (randomColor)
        {
            _modelRenderer.material.color = GenerateRandomColor();
        }
        // attach nav mesh agent
        if (usePathPlanning)
        {
            _navMeshAgent = gameObject.AddComponent(typeof(NavMeshAgent)) as NavMeshAgent;
        }
        _attachedObjects = new List<GameObject>();
        
        // create states dictionary
        _objectStatesContent = new Dictionary<string, string>();
        _objectStates = new Dictionary<string, bool>();
        // set HUD references
        var texts = _stateCanvas.GetComponentsInChildren<Text>();
        _nameText = texts[0];
        _statesText = texts[1];
        
        // set up HUD messages
        _nameText.text = "Object "  + "'" +gameObject.name +"'";
        _statesText.text = "";
    }
    
    /// <summary>
    /// Generate Random color for 3D model material
    /// </summary>
    /// <returns>Material's color</returns>
    private static Color GenerateRandomColor()
    {
        return new Color(
            Random.Range(0f,1f),
            Random.Range(0f,1f),
            Random.Range(0f,1f)
            );
    }

    public void SetState(bool negated, string stateName, string content)
    {
        _objectStates[stateName] = negated;
        _objectStatesContent[stateName] = content;
        UpdateStateCanvas();
    }
    
    /// <summary>
    /// Update HUD messages with states status
    /// </summary>
    private void UpdateStateCanvas()
    {
        var message = "";
        foreach (var key in _objectStates.Keys)
        {
            message +=((_objectStates[key]? "not " : "")) + key + " " + _objectStatesContent[key] + "\n";
        }

        message.Remove(message.Length - 1);
        _statesText.text = message;
    }
    
    /// <summary>
    /// Activate HUD object canvas on mouse exit
    /// </summary>
    private void OnMouseEnter()
    {
        _stateCanvas.SetActive(true);
    }
    
    /// <summary>
    /// Deactivate HUD object canvas on mouse exit
    /// </summary>
    private void OnMouseExit()
    {
        _stateCanvas.SetActive(false);
    }
    /* ================================= */
    /* ========== ANIMATIONS =========== */
    /* ================================= */

    /// <summary>
    /// Simple move method
    /// </summary>
    /// <param name="target">Target point in space (Vector3)</param>
    /// <param name="instant">instant movement</param>
    /// <returns>Coroutine</returns>
    public IEnumerator Move(Vector3 target, bool instant = false)
    {
        StopAllCoroutines();
        yield return StartCoroutine(MoveTo(transform.position, target,instant));
    }
    
    /// <summary>
    /// Move the object to another object considering the offsets alignments 
    /// </summary>
    /// <param name="targetObject">Target object</param>
    /// <param name="alignment">Alignment enumerator</param>
    /// <param name="offset">Offset Vector3</param>
    /// <returns></returns>
    public IEnumerator MoveToObjectAlignedTo(GenericObject targetObject, Alignment alignment = Alignment.None, Vector3 offset = new Vector3(), bool instant = false)
    {
        StopAllCoroutines();
        switch (alignment)
        {
            case Alignment.Top:
                yield return StartCoroutine(MoveTo(_bottom.position, targetObject._top.position + offset,instant));
                break;
            case Alignment.Bottom:
                yield return StartCoroutine(MoveTo(_top.position, targetObject._bottom.position + offset,instant));
                break;
            case Alignment.Left:
                yield return StartCoroutine(MoveTo(_right.position, targetObject._left.position + offset,instant));
                break;
            case Alignment.Right:
                yield return StartCoroutine(MoveTo(_left.position, targetObject._right.position + offset,instant));
                break;
            case Alignment.Front:
                yield return StartCoroutine(MoveTo(_back.position, targetObject._front.position + offset,instant));
                break;
            case Alignment.Back:
                yield return StartCoroutine(MoveTo(_front.position, targetObject._back.position + offset,instant));
                break;
            default:
                yield return StartCoroutine(MoveTo(transform.position, targetObject.transform.position + offset,instant));
                break;
        }

        yield return null;
    }

    /// <summary>
    /// Main movement Coroutine
    /// </summary>
    /// <param name="start">Start point in world</param>
    /// <param name="target">Target point in world</param>
    /// <param name="instant">instant movement</param>
    /// <returns></returns>
    private IEnumerator MoveTo(Vector3 start, Vector3 target, bool instant)
    {
        var position1 = transform.position;
        var pointOffset = position1 - start;
        var newTarget = target + pointOffset;

        if (!usePathPlanning)
        {
            if (!instant)
            {
                var offSet = position1 - newTarget;
                var sqrOffSet = offSet.sqrMagnitude;
                while (sqrOffSet > 0.05f)
                {
                    var position = transform.position;
                    position = Vector3.Lerp(position, newTarget, 0.2f);
                    transform.position = position;
                    offSet = position - newTarget;
                    sqrOffSet = offSet.sqrMagnitude;
                    yield return new WaitForSeconds(0.1f);
                }
            }
            transform.position = newTarget;
        }
        else
        {
            _navMeshAgent.SetDestination(newTarget);
            while (true)
            {
                if (!_navMeshAgent.pathPending && _navMeshAgent.remainingDistance < 0.03f)
                {
                    break;
                }
                yield return null;
            }
        }
        
        yield return null;
    }
    
    /// <summary>
    /// Draw debug gizmos
    /// </summary>
    private void OnDrawGizmos()
    {
        if (!showDebugGizmos) return;
        Gizmos.color = Color.red;
        var position = transform.position;
        Gizmos.DrawWireCube(position, Vector3.one);
        Gizmos.color = new Color(1.0f, 0.0f, 0.0f, 0.5f);
        Gizmos.DrawCube(position, Vector3.one);
        Gizmos.color = Color.gray;
    }

    /// <summary>
    /// Set new color to mesh 3D model
    /// </summary>
    /// <param name="newColor"></param>
    public void ChangeColor(Color newColor)
    {
        _modelRenderer.material.color = newColor;
    }
    
    /// <summary>
    /// Get Alignment offset's position in the world
    /// </summary>
    /// <param name="alignment">Alignment enumerator</param>
    /// <returns></returns>
    public Vector3 GetAlignmentPoint(Alignment alignment)
    {
        switch (alignment)
        {
            case Alignment.Top:
                return _top.position;
            case Alignment.Bottom:
                return _bottom.position;
            case Alignment.Left:
                return _left.position;
            case Alignment.Right:
                return _right.position;
            case Alignment.Front:
                return _front.position;
            case Alignment.Back:
                return _back.position;
            case Alignment.None:
                return transform.position;
            default:
                return _top.position;
        }
    }

    public void Attach(GameObject other)
    {
        _attachedObjects.Add(other);
        other.transform.parent = gameObject.transform;
    }

    public void Detach(GameObject other)
    {
        var index =_attachedObjects.IndexOf(other);
        if (index <= -1) return;
        var holder = GameObject.Find("Simulation Objects");
        _attachedObjects[index].transform.parent = holder.transform;
        _attachedObjects.RemoveAt(index);
    }
}
