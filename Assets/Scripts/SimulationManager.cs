using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
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
    private Dictionary<string, PddlDomainPredicate> _predicatesDictionary;
    
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
        _predicatesDictionary = new Dictionary<string, PddlDomainPredicate>();
        foreach (var predicate in simulationSettings.predicates)
        {
            _predicatesDictionary.Add(predicate.predicateName.ToLower(), predicate);
        }

        //CREATE OBJECTS TYPES DICTIONARY
        _objectsTypeDictionary = new Dictionary<string, PddlType>();
        foreach (var problemObject in simulationEnvironment.objects)
        {
            //check type tree
            var type = simulationSettings.types.FindIndex(t => t.typeName == problemObject.objectType.typeName);
            _objectsTypeDictionary.Add(problemObject.objectName.ToLower(), simulationSettings.types[type]);
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
            var commandsSettings = _predicatesDictionary[predicateName].predicateCommandSettings;
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
                        _objectsTypeDictionary[objCalled].typeName)
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
        // for each effect
        // group commands by order of execution
        // for each group
        //for each command of same order
        // start command coroutine and wait for all to finish
        
        //NEW
        foreach (var action in simulationEnvironment.plan.actions)
        {
            var effects = simulationSettings.GetActionEffects(action.actionName);
            var executionDictionary = new Dictionary<int, List<Execution>>();
            foreach (var effect in effects)
            {
                // get predicate name
                var predicateName = effect.predicateName.ToLower();
                // get all command settings
                var commandsSettings = _predicatesDictionary[predicateName].predicateCommandSettings;
                // if no command continue loop
                if (commandsSettings.Count <= 0) continue;
                
                // group commands by order of execution
                var executions = commandsSettings.GroupBy(c => c.orderOfExecution);
                // for each group of commands
                foreach (var group in executions)
                {
                    if (!executionDictionary.TryGetValue(group.Key, out var executionList))
                    {
                        executionList = new List<Execution>();
                        executionDictionary[group.Key] = executionList;
                    }
                    // for each command in the same execution order
                    foreach (var commandSettings in group)
                    {
                        var typeNameOfFirstParameterSetOnCommand = _predicatesDictionary[predicateName].parametersTypes[0];
                        
                        var firstParameterObjectName = action.parameters[effect.attributesIndexes[0]].ToLower();
                        var firstParameterObject = GetObject(firstParameterObjectName);
                        var typeNameOfFirstParameterOfEffect = _objectsTypeDictionary[firstParameterObject.name].typeName;

                        var typeOfFirstParameterOfEffect = _objectsTypeDictionary[firstParameterObjectName];
                        var parentTypeOfFirstParameterOfEffect = typeOfFirstParameterOfEffect.parentTypeName;
                        // if same type or children
                        if (typeNameOfFirstParameterOfEffect == typeNameOfFirstParameterSetOnCommand ||
                            parentTypeOfFirstParameterOfEffect == typeNameOfFirstParameterSetOnCommand
                        )
                        {
                            var parameters = new List<string>();
                            foreach (var index in effect.attributesIndexes)
                            {
                                parameters.Add(action.parameters[index]);
                            }
                            executionList.Add(new Execution(commandSettings.commandBehavior, parameters, effect.isNegated));
                        }
                    }
                }
            }
            
            // execute effects
            foreach (var executionOrder in executionDictionary.Keys)
            {
                var executingCommands = new List<Coroutine>();
                // start all same order coroutines
                foreach (var execution in executionDictionary[executionOrder])
                {
                    var objects = GetObjects(execution.parameterObjects);
                    execution.commandCoroutine.SetAttributes(objects);
                    yield return execution.commandCoroutine.Execute(execution.negatedEffect);
                }
                //wait all the coroutines to finish
                // foreach(var command in executingCommands)
                // {
                //     yield return command;
                // }
            }
            
        }


        // OLD
        // foreach (var action in simulationEnvironment.plan.actions)
        // {
        //     var effects = simulationSettings.GetActionEffects(action.actionName);
        //     foreach (var effect in effects)
        //     {
        //         // get predicate name
        //         var predicateName = effect.predicateName.ToLower();
        //         // get all command settings
        //         var commandsSettings = _predicatesDictionary[predicateName].predicateCommandSettings;
        //         // if no command continue loop
        //         if (commandsSettings.Count <= 0) continue;
        //
        //         // todo move in predicate
        //         // get first object on the action parameters
        //         var theObject = GetObject(action.parameters[effect.attributesIndexes[0]].ToLower());
        //         // change its state description in HUD
        //         theObject.SetState(effect.predicateName, effect.isNegated);
        //
        //         // group commands by order of execution
        //         var executions = commandsSettings.GroupBy(c => c.orderOfExecution);
        //
        //         // for each group of commands
        //         foreach (var group in executions)
        //         {
        //             // coroutine list to start
        //             var coroutineToStart = new List<PredicateCommand>();
        //             // for each command with same order of execution
        //             foreach (var order in @group)
        //             {
        //                 var typeNameOfFirstParameterSetOnCommand = simulationEnvironment.types[order.predicateTypeIndex];
        //                 typeNameOfFirstParameterSetOnCommand = _predicatesDictionary[predicateName].parametersTypes[0];
        //                 
        //                 var firstParameterObjectName = action.parameters[effect.attributesIndexes[0]].ToLower();
        //                 var firstParameterObject = GetObject(firstParameterObjectName);
        //                 var typeNameOfFirstParameterOfEffect = _objectsTypeDictionary[firstParameterObject.name].typeName;
        //
        //                 var typeOfFirstParameterOfEffect = _objectsTypeDictionary[firstParameterObjectName];
        //                 var parentTypeOfFirstParameterOfEffect = typeOfFirstParameterOfEffect.parentTypeName;
        //                 // if same type or children
        //                 if(typeNameOfFirstParameterOfEffect == typeNameOfFirstParameterSetOnCommand ||
        //                    parentTypeOfFirstParameterOfEffect == typeNameOfFirstParameterSetOnCommand
        //                    )
        //                 {
        //                     Debug.Log(firstParameterObjectName);
        //                     coroutineToStart.Add(order.commandBehavior);
        //                 }
        //
        //                 
        //             }
        //             // starts all coroutines
        //             foreach (var command in coroutineToStart)
        //             {
        //                 var parameters = new List<string>();
        //                 foreach (var index in effect.attributesIndexes)
        //                 {
        //                     parameters.Add(action.parameters[index]);
        //                 }
        //
        //                 var objects = GetObjects(parameters);
        //                 command.SetAttributes(objects);
        //                 yield return command.Execute(effect.isNegated);
        //             }
        //         }
        //     }
        // }

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
            var clone = Instantiate(gameObj, simulationEnvironment.objectsPositions[i], Quaternion.identity,
                _objectsHolder);
            clone.name = simulationEnvironment.objects[i].objectName;
        }
    }

    private IEnumerator GeneratePlan()
    {
        var plan = new Plan();
        yield return _planSolver.RequestPlan(
            simulationSettings.domain.text,
            simulationEnvironment.problem.text,
            value => plan = value);
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


    private class Execution
    {
        public List<string> parameterObjects;
        public PredicateCommand commandCoroutine;
        public bool negatedEffect;
        public Execution(PredicateCommand commandCoroutine, List<string> parameterObjects, bool negatedEffect)
        {
            this.commandCoroutine = commandCoroutine;
            this.parameterObjects = parameterObjects;
            this.negatedEffect = negatedEffect;
        }
    }
}