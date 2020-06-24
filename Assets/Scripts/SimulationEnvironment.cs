using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

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
    [SerializeField] public List<PddlInit> initBlock;

    // =====================
    // UNITY REPRESENTATIONS
    // =====================
    
    // name of simulation
    [SerializeField] public string simulationName;
    
    // Model mesh or game object that represent a PDDL type in the scene
    [SerializeField] public GameObject[] typesModels;

    // Start transform at instantiation
    [SerializeField] public Vector3[] objectsPositions;

    // ==============
    // PUBLIC METHODS
    // ==============

    public GameObject GetPrefabWithType(string typeName)
    {
        var index = types.FindIndex(a => a.Contains(typeName));
        return index != -1 ? typesModels[index] : null;
    }

    public int GetObjectIndexPosition(string objName)
    {
        return objects.FindIndex(a => a.name.Equals(name));
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