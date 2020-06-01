using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FaceCamera : MonoBehaviour
{
    public Canvas canvas;
    private void Start()
    {
        canvas.worldCamera = Camera.main;
    }
    void Update()
    {
        transform.LookAt(transform.position + Camera.main.transform.rotation * Vector3.forward, 
                         canvas.worldCamera.transform.rotation * Vector3.up);
    }
}
