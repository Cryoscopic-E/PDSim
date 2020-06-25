using System.IO;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEditor;

public class SimulationManager : MonoBehaviour
{
    private readonly string SIM_OBJECT_HOLDER = "Simulation Objects";
    private readonly string CUSTOM_OBJECT_HOLDER = "Custom Objects";
    
    public SimulationSettings simulationSettings;
    public SimulationEnvironment simulationEnvironment;


    private Transform _objectsHolder;
    private Transform _customHolder;
    
    private Dictionary<string, GenericObject> objectsDictionary;
    private PlanSolver _planSolver;
    private HudController _hudController;
    private void Start()
    {
        _hudController = FindObjectOfType<HudController>();
        _hudController.SetCurrentAction("--", "--");
        _hudController.SetSimulationStatus(SimStatus.None);
        
        _planSolver = GetComponent<PlanSolver>();
        // SET holders game objects in scene
        SetHolders();
        // SETUP SIMULATION WITH SETTINGS
        SetupSimulation();
        // START SIMULATION ROUTINES
        StartCoroutine(SimulatePlan());
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
        // Check if plan exist, or generate
        if (simulationEnvironment.plan.actions.Count == 0)
        {
            yield return GeneratePlan();
        }
        
        // Update HUD
        _hudController.SetSimulationStatus(SimStatus.Init);
        // Run Initialization block
        yield return InitSimulation();
        
        // Update HUD
        _hudController.SetSimulationStatus(SimStatus.PlanExecution);
        // Plan execution
        yield return PlanExecution();
        
        // Update HUD
        _hudController.SetSimulationStatus(SimStatus.None);
        // Simulation end
        yield return null;
    }
    
    private IEnumerator InitSimulation()
    {
        //TODO run initialization block
        yield return null;
    }

    private IEnumerator PlanExecution()
    {
        foreach (var action in simulationEnvironment.plan.actions)
        {
            var effects = simulationSettings.GetActionEffects(action.name);

            foreach (var predicate in effects)
            {
                // check action behaviour
                var behaviour = simulationSettings.GetPredicateBehaviour(predicate.predicateName);
                
                if (!behaviour) continue;
                
                // get parameters
                var param = GetObjects(action.parameters);
                behaviour.SetAttributes(param);
                
                // update HUD actions
                _hudController.SetCurrentAction(action.name, predicate.predicateName);
                
                //TODO update HUD on object
                // run predicate command
                yield return behaviour.Execute(predicate.negate);
            }
        }
        yield return null;
    }

    // CALLED FROM INSPECTOR
    public void GenerateScene()
    {
        SetHolders();
        
        // Generate Simulation Objects
        for (int i = 0; i < simulationEnvironment.objects.Count; i++)
        {
            var gameObj = simulationEnvironment.GetPrefabWithType(simulationEnvironment.objects[i].type);
            if (gameObj == null) continue;
            var clone = Instantiate(gameObj, simulationEnvironment.objectsPositions[i], Quaternion.identity, _objectsHolder);
            clone.name = simulationEnvironment.objects[i].name;
        }
    }
    private IEnumerator GeneratePlan()
    {
        var plan = new Plan();
        yield return _planSolver.RequestPlan(
            simulationSettings.domain.text, 
            simulationEnvironment.problem.text, 
            value=> plan = value);
        simulationEnvironment.SavePlan(plan);
        yield return null;
    }

    private List<GenericObject> GetObjects(List<string> names)
    {
        List<GenericObject> list = new List<GenericObject>();
        for (int i = 0; i < names.Count; i++)
        {
            list.Add(objectsDictionary[names[i]]);
        }

        return list;
    }

    private void SetHolders()
    {
        if (_objectsHolder != null || _customHolder != null) return;
        _objectsHolder = GetHolder(SIM_OBJECT_HOLDER);
        _customHolder = GetHolder(CUSTOM_OBJECT_HOLDER);
    }
    
    private Transform GetHolder(string name)
    {
        var holder = GameObject.Find(name);
        if (!holder)
        {
            holder = new GameObject(name);
        }
        return holder.transform;
    }


    public void Play()
    {
        Time.timeScale = 1.0f;
    }

    public void Pause()
    {
        Time.timeScale = 0.0f;
    }

    public void SaveEnvironment()
    {
        SetHolders();
        //for each object in object holder
        for (int i = 0; i < _objectsHolder.childCount; i++)
        {
            // get object name
            var obj = _objectsHolder.GetChild(i);
            // get index in environment object list
            var index = simulationEnvironment.GetObjectIndexPosition(obj.name);
            // save position
            simulationEnvironment.objectsPositions[i] = obj.transform.position;
        }
        simulationEnvironment.Save();
    }
}