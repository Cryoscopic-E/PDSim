using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class SimulationManager : MonoBehaviour
{
    public SimulationSettings simulationSettings;
    public Plan plan;
    
    
    private Transform _objectsHolder;
    
    private Dictionary<string, GenericObject> objectsDictionary;

    private void Start()
    {
        CheckObjectsHolder();
        
        // SETUP SIMULATION WITH SETTINGS
        SetupSimulation();
        // START SIMULATION ROUTINES
        //StartCoroutine(SimulatePlan());
    }

    private void SetupSimulation()
    {
        // ADD PROBLEM OBJECTS TO DICTIONARY
        objectsDictionary = new Dictionary<string, GenericObject>();
        for (int i = 0; i < _objectsHolder.childCount; i++)
        {
            GenericObject obj = _objectsHolder.GetChild(i).GetComponent<GenericObject>();
            objectsDictionary.Add(obj.gameObject.name, obj);
        }

    }
    private IEnumerator SimulatePlan()
    {
        foreach (PlanAction action in plan.actions)
        {
            // foreach (string actionName in simulationSettings.pddlElements.actions)
            // {
            //     if (actionName.ToLower() == action.name)
            //     {
            //         yield return null; //TODO return action behaviour coroutine
            //     }
            // }
        }

        yield return null;
    }

    private void CheckObjectsHolder()
    {
        var objs = GameObject.Find("Objects");
        if (!objs)
        {
            objs = new GameObject("Objects");
        }
        _objectsHolder = objs.GetComponent<Transform>();
    }
    
    public void GenerateScene()
    {
        CheckObjectsHolder();
        foreach (var obj in simulationSettings.pddlElements.objects)
        {
            
        }
    }

}