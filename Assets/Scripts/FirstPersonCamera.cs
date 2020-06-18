using System;
using UnityEngine;


public class FirstPersonCamera : MonoBehaviour
{
    public float strafeSpeed = 50.0f;
    public float moveSpeed = 50.0f;

    private Vector3 _direction;
    private float _delta;
    private float _previousTime;

    public void Awake()
    {
        _previousTime = Time.realtimeSinceStartup;
    }

    public void Update()
    {
        /* Calculate delta time to be time indipendent*/
        var currentTime = Time.realtimeSinceStartup;
        _delta = currentTime - _previousTime;
        _previousTime = currentTime;

        //It is possible (especially if this script is attached to an object that is 
        //created when the scene is loaded) that the calculated delta time is 
        //less than zero.  In that case, discard this update.
        if(_delta < 0)
        {
            _delta = 0;
        }
        
        _direction.x = Input.GetAxisRaw("Horizontal") * strafeSpeed;
        _direction.z = Input.GetAxisRaw("Vertical") * moveSpeed;
        transform.Translate(_direction * _delta);
    }
}
