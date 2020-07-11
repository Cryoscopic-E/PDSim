using UnityEngine;

public class FirstPersonCamera : MonoBehaviour
{
    public float moveSpeed = 20.0f;
    public float lookSpeed = 55.0f;
 
    public float rotX = 0.0f;
    public float rotY = 0.0f;
    private Vector3 _direction;
    private float _delta;
    private float _previousTime;

    private bool controlEnabled = false;
    
    public void Awake()
    {
        _previousTime = Time.realtimeSinceStartup;
    }

    public void LateUpdate()
    {
        /* Calculate delta time to be time indipendent*/
        var currentTime = Time.realtimeSinceStartup;
        _delta = currentTime - _previousTime;
        _previousTime = currentTime;
        if(_delta < 0)
        {
            _delta = 0;
        }
        
        if(Input.GetKeyDown(KeyCode.C))
            controlEnabled = !controlEnabled;

        if (controlEnabled)
        {
            rotX += Input.GetAxis("Mouse X")*lookSpeed;
            rotY += Input.GetAxis("Mouse Y")*lookSpeed;
            rotY = Mathf.Clamp (rotY, -90, 90);
 
            transform.localRotation = Quaternion.AngleAxis(rotX, Vector3.up);
            transform.localRotation *= Quaternion.AngleAxis(rotY, Vector3.left);
 
            transform.position += transform.forward*moveSpeed*Input.GetAxis("Vertical")*_delta;
            transform.position += transform.right*moveSpeed*Input.GetAxis("Horizontal")*_delta;
        }
        
        
    }
}
