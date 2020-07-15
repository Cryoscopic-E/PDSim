using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SimulationManager : MonoBehaviour
{
    private readonly string SIM_OBJECT_HOLDER = "Simulation Objects";
    private readonly string CUSTOM_OBJECT_HOLDER = "Custom Objects";
    
    public SimulationSettings simulationSettings;
    public SimulationEnvironment simulationEnvironment;
    

    private Transform _objectsHolder;
    private Transform _customHolder;
    
    private Dictionary<string, GenericObject> _objectsDictionary;
    private Dictionary<string, PddlType> _objectsTypeDictionary;
    private Dictionary<string, List<PredicateCommandSettings>> _predicatesCommandsDictionary;
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
        _objectsDictionary = new Dictionary<string, GenericObject>();
        for (var i = 0; i < _objectsHolder.childCount; i++)
        {
            var obj = _objectsHolder.GetChild(i).GetComponent<GenericObject>();
            _objectsDictionary.Add(obj.gameObject.name, obj);
        }
        // CREATE PREDICATES DICTIONARY
        _predicatesCommandsDictionary = new Dictionary<string, List<PredicateCommandSettings>>();
        foreach (var predicate in simulationSettings.predicates)
        {
            var orderCommandsExecution = predicate.predicateCommandSettings
                .OrderBy(p => p.orderOfExecution).ToList();
            _predicatesCommandsDictionary.Add(predicate.predicateName.ToLower(), orderCommandsExecution);
        }
        
        //CREATE OBJECTS TYPES DICTIONARY
        _objectsTypeDictionary = new Dictionary<string, PddlType>();
        foreach (var problemObject in simulationEnvironment.objects)
        {
            _objectsTypeDictionary.Add(problemObject.objectName.ToLower(), problemObject.objectType);
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
        foreach (var predicate in simulationEnvironment.initBlock)
        {
            var predicateName = predicate.predicateName.ToLower();
            var commandsSettings = _predicatesCommandsDictionary[predicateName];
            // execute commands only if they have been defined
            if (commandsSettings.Count <= 0) continue;
            
            var objCalled = predicate.objectParameters[0].ToLower();
                
            //HUD UPDATE in object
            var genericObject = GetObject(objCalled);
            genericObject.SetState(predicate.predicateName, predicate.isNegated);
            // group by execution order
            var executions = commandsSettings.GroupBy(c => c.orderOfExecution);

            foreach (var group in executions)
            {
                var coroutineToStart = new List<PredicateCommand>();
                foreach (var order in @group)
                {
                    //if same type or children
                    if (simulationEnvironment.types[order.predicateTypeIndex] ==
                        _objectsTypeDictionary[objCalled].typeName )
                        // ||
                        // simulationEnvironment.types[order.predicateTypeIndex] ==
                        // _objectsTypeDictionary[objCalled].parentType.typeName)
                    {
                        coroutineToStart.Add(order.commandBehavior);
                    }
                    coroutineToStart.Add(order.commandBehavior);
                }
                // starts all coroutines
                foreach (var command in coroutineToStart)
                {
                    var objects = GetObjects(predicate.objectParameters);
                    command.SetAttributes(objects);
                    yield return command.Execute(predicate.isNegated);
                }
            }
        }
        yield return null;
    }

    private IEnumerator PlanExecution()
    {
        foreach (var action in simulationEnvironment.plan.actions)
        {
            Debug.Log(action.actionName);
            var effects = simulationSettings.GetActionEffects(action.actionName);
            foreach (var effect in effects)
            {
                var predicateName = effect.predicateName.ToLower();
                var commandsSettings = _predicatesCommandsDictionary[predicateName];
                
                if (commandsSettings.Count <= 0) continue;
                
                
                var theObject = GetObject(action.parameters[effect.attributesIndexes[0]].ToLower());
                theObject.SetState(effect.predicateName, effect.isNegated);
                
                var executions = commandsSettings.GroupBy(c => c.orderOfExecution);

                foreach (var group in executions)
                {
                    var coroutineToStart = new List<PredicateCommand>();
                    foreach (var order in @group)
                    {
                        // if same type or children
                        if (simulationEnvironment.types[order.predicateTypeIndex] ==
                            _objectsTypeDictionary[theObject.name].typeName)
                            // ||
                            // simulationEnvironment.types[order.predicateTypeIndex] ==
                            // _objectsTypeDictionary[objCalled].parentType.typeName)
                        {
                            
                        }
                        coroutineToStart.Add(order.commandBehavior);
                    }
                    // starts all coroutines
                    foreach (var command in coroutineToStart)
                    {
                        var parameters = new List<string>();
                        foreach (var indx in effect.attributesIndexes)
                        {
                            Debug.Log(action.parameters[indx]);
                            parameters.Add(action.parameters[indx]);
                        }
                        var objects = GetObjects(parameters);
                        command.SetAttributes(objects);
                        yield return command.Execute(effect.isNegated);
                    }
                }
            }
        }
        yield return null;
    }

    // CALLED FROM INSPECTOR
    public void GenerateScene()
    {
        //TODO check types from problem with user defined types
        
        SetHolders();
        
        // Generate Simulation Objects
        for (int i = 0; i < simulationEnvironment.objects.Count; i++)
        {
            var gameObj = simulationSettings.GetPrefabWithType(simulationEnvironment.objects[i].objectType);
            if (gameObj == null) continue;
            var clone = Instantiate(gameObj, simulationEnvironment.objectsPositions[i], Quaternion.identity, _objectsHolder);
            clone.name = simulationEnvironment.objects[i].objectName;
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

    private GenericObject GetObject(string name)
    {
        return _objectsDictionary[name];
    }
    
    private List<GenericObject> GetObjects(List<string> names)
    {
        var list = new List<GenericObject>();
        for (int i = 0; i < names.Count; i++)
        {
            list.Add(_objectsDictionary[names[i].ToLower()]);
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