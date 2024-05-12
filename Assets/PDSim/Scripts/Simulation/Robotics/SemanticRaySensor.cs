using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class SemanticRaySensor : MonoBehaviour
{
    public bool showDebug;
    [Space]
    [Header("Modify Proposition")]
    public string modProposition;

    public SemanticValue setValueOnRayHit;

    [Space]
    [Header("Ray Properties")]

    public string filterTag;

    public float lenght;

    public CardinalDirection cardinalDirection;

    public Vector3 offSet;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnDrawGizmos()
    {
        Gizmos.DrawRay(transform.position, -transform.up * lenght);
    }

    public enum CardinalDirection
    {
        UP,
        DOWN,
        LEFT,
        RIGHT,
        FORWARD,
        BACKWARD
    }

    public enum SemanticValue
    {
        True,
        False
    }
}
