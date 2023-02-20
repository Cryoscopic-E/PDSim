using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
[CreateAssetMenu(fileName = "Simulation Environment")]
public class SimulationEnvironment : ScriptableObject
{
    // ================
    // PROBLEM SETTINGS
    // ================

    // The problem file
    [SerializeField] public TextAsset problem;

    // Types in current environment
    [SerializeField] public List<string> types;

    // The plan associated to the problem
    [SerializeField] public Plan plan;

    // Objects to instantiate with respective type
    [SerializeField] public List<PddlObject> objects;

    // Initialization block
    [SerializeField] public List<PddlInitPredicate> initBlock;

    // =====================
    // UNITY REPRESENTATIONS
    // =====================

    // Start transform at instantiation
    [SerializeField] public List<Vector3> objectsPositions;

    // ==============
    // PUBLIC METHODS
    // ==============


    public void Reset()
    {
        plan = null;
        objects = null;
        types = null;
        objectsPositions = null;
        initBlock = null;
    }

    public int GetObjectIndexPosition(string objName)
    {
        return objects.FindIndex(a => a.objectName.Equals(name));
    }


    public void SavePlan(Plan plan)
    {
        this.plan = plan;
        Save();
    }

    public void Save()
    {
        EditorUtility.SetDirty(this);
    }
}