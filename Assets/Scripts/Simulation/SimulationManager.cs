using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;

public class SimulationManager : MonoBehaviour
{
    [HideInInspector]
    public readonly string SIM_OBJECT_HOLDER = "Simulation Objects";
    [HideInInspector]
    public readonly string CUSTOM_OBJECT_HOLDER = "Custom Objects";

    public SimulationEnvironment simulationEnvironment;
    
    private Transform _objectsHolder;
    private Transform _customHolder;
    private PlanSolver _planSolver;
    private HudController _hudController;

    private Dictionary<string, GenericObject> _objectsDictionary;
    private Dictionary<string, PddlType> _objectsTypeDictionary;
    private Dictionary<string, PddlDomainPredicate> _predicatesDictionary;
    
    

    // ===============
    // DOMAIN SETTINGS
    // ===============
    
    // The domain file
    [SerializeField] public TextAsset domain;
    // Domain's list of actions
    [SerializeField] public List<PddlAction> actions;
    // Domain's list of predicates
    [SerializeField] public List<PddlDomainPredicate> predicates;
    // Domain's list of types
    [SerializeField] public List<string> typesToDefine;
    [SerializeField] public List<int> typesDefined; //maps indexes to typesToDefine
    [SerializeField] public List<PddlType> types;
    
    
    // name of simulation
    [SerializeField] public string simulationName;
    

    // Generic object models for types
    [SerializeField] public List<GameObject> typesGameObject;
    
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
            _objectsDictionary.Add(obj.gameObject.name.ToLower(), obj);
        }

        // CREATE PREDICATES DICTIONARY
        _predicatesDictionary = new Dictionary<string, PddlDomainPredicate>();
        foreach (var predicate in predicates)
        {
            _predicatesDictionary.Add(predicate.predicateName.ToLower(), predicate);
        }

        //CREATE OBJECTS TYPES DICTIONARY
        _objectsTypeDictionary = new Dictionary<string, PddlType>();
        foreach (var problemObject in simulationEnvironment.objects)
        {
            //check type tree
            var type = types.FindIndex(t => t.typeName == problemObject.objectType.typeName);
            _objectsTypeDictionary.Add(problemObject.objectName.ToLower(), types[type]);
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
            // HUD update
            _hudController.SetCurrentAction("---", predicateName);
            var objCalled = predicate.objectParameters[0].ToLower();

            
            // group by execution order
            var executions = commandsSettings
                .OrderBy(c=>c.orderOfExecution)
                .GroupBy(c => c.orderOfExecution);

            foreach (var group in executions)
            {
                var coroutineToStart = new List<CommandBase>();
                foreach (var order in @group)
                {
                    //if same type or children
                    var typeNameOfFirstParameterSetOnCommand = _predicatesDictionary[predicateName].parametersTypes[order.predicateTypeIndex];
                        
                    var firstParameterObjectName = predicate.objectParameters[0];
                    var firstParameterObject = GetObject(firstParameterObjectName);
                    var typeNameOfFirstParameterOfEffect = _objectsTypeDictionary[firstParameterObject.name].typeName;

                    var typeOfFirstParameterOfEffect = _objectsTypeDictionary[firstParameterObjectName];
                    var parentTypeOfFirstParameterOfEffect = typeOfFirstParameterOfEffect.parentTypeName;
                   
                    if (typeNameOfFirstParameterOfEffect == typeNameOfFirstParameterSetOnCommand ||
                        parentTypeOfFirstParameterOfEffect == typeNameOfFirstParameterSetOnCommand
                    )
                    {
                        coroutineToStart.Add(order.commandBaseBehavior);
                    }
                }

                // starts all coroutines
                foreach (var command in coroutineToStart)
                {
                    var objects = GetObjects(predicate.objectParameters);
                    if (objects.Count > 0)
                    {
                        var genericObject = objects[0];
                        var otherObjects = objects.GetRange(1, objects.Count - 1);
                        var objectsNames = "";
                        foreach (var o in otherObjects)
                        {
                            objectsNames += o.name + " ";
                        }
                        genericObject.SetState(predicate.isNegated, predicate.predicateName, objectsNames);
                        command.Init(objects);
                        yield return command.Execute(predicate.isNegated);
                    }
                    
                    
                }
            }
        }

        yield return null;
    }

    private IEnumerator PlanExecution()
    {
        //NEW
        foreach (var action in simulationEnvironment.plan.actions)
        {
            var effects = GetActionEffects(action.actionName);
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
                var executions = commandsSettings
                    .OrderBy(c=>c.orderOfExecution)
                    .GroupBy(c => c.orderOfExecution);
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
                        var typeNameOfFirstParameterSetOnCommand = _predicatesDictionary[predicateName].parametersTypes[commandSettings.predicateTypeIndex];
                        
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

                            var commandClone = Instantiate(commandSettings.commandBaseBehavior) as CommandBase;
                            executionList.Add(new Execution(predicateName, commandClone, parameters, effect.isNegated));
                        }
                    }
                }
            }
            
            // execute effects
            foreach (var executionOrder in executionDictionary.Keys)
            {
                var executingCommands = new List<Coroutine>();
                // start all same order coroutines
                var predicatesNames = "";
                foreach (var execution in executionDictionary[executionOrder])
                {
                    predicatesNames += ((execution.negatedEffect)? "not(" : "(") + execution.predicate + ")  ";
                    var objects = GetObjects(execution.parameterObjects);
                    execution.CommandBaseCoroutine.Init(objects);
                    executingCommands.Add(StartCoroutine(execution.CommandBaseCoroutine.Execute(execution.negatedEffect)));
                    
                    
                    var genericObject = objects[0];
                    var otherObjects = objects.GetRange(1, objects.Count - 1);
                    var objectsNames = "";
                    foreach (var o in otherObjects)
                    {
                        objectsNames += o.name + " ";
                    }
                    genericObject.SetState(execution.negatedEffect, execution.predicate, objectsNames);
                }
                // HUD Update
                _hudController.SetCurrentAction(action.actionName, predicatesNames);
                //wait all the coroutines to finish
                foreach(var command in executingCommands)
                {
                     yield return command;
                }
                
                       
            }
            
        }
        yield return null;
    }
    
    public void GenerateScene()
    {
        SetHolders();
        // Generate Simulation Objects
        for (int i = 0; i < simulationEnvironment.objects.Count; i++)
        {
            var gameObj = GetPrefabWithType(simulationEnvironment.objects[i].objectType);
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
            domain.text,
            simulationEnvironment.problem.text,
            value => plan = value);
        simulationEnvironment.SavePlan(plan);
        yield return null;
    }

    private GenericObject GetObject(string name)
    {
        return _objectsDictionary[name.ToLower()];
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

    public void SetHolders()
    {
        if (_objectsHolder != null || _customHolder != null) return;
        _objectsHolder = GetHolder(SIM_OBJECT_HOLDER);
        _customHolder = GetHolder(CUSTOM_OBJECT_HOLDER);
    }

    public Transform GetHolder(string name)
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
        public string predicate;
        public List<string> parameterObjects;
        public CommandBase CommandBaseCoroutine;
        public bool negatedEffect;
        public Execution(string predicate, CommandBase commandBaseCoroutine, List<string> parameterObjects, bool negatedEffect)
        {
            this.predicate = predicate;
            this.CommandBaseCoroutine = commandBaseCoroutine;
            this.parameterObjects = parameterObjects;
            this.negatedEffect = negatedEffect;
        }
    }
    
    
    public List<PredicateCommandSettings> GetPredicateCommandSettings(string predicateName)
    {
        var index = predicates.FindIndex(a => a.predicateName.Contains(predicateName));
        return predicates[index].predicateCommandSettings;
    }
    
    public List<PddlEffectPredicate> GetActionEffects(string actionName)
    {
        foreach (var action in actions)
        {
            if (string.Equals(action.actionName, actionName, StringComparison.CurrentCultureIgnoreCase))
            {
                return action.effects;
            }
        }
        return null;
    }
    
    public GameObject GetPrefabWithType(PddlType type)
    {
        var index = typesToDefine.FindIndex(a => a.Contains(type.typeName));

        var definedIndex = typesDefined.FindIndex(t => t == index);
        return index != -1 ? typesGameObject[definedIndex] : null;
    }

    public void Initialize()
    {
        // parse elements
        Parser.ParseDomain(
            domain.text,
            out types,
            out predicates,
            out  actions);
                    
        // Create list of types string to define from original list of types
        typesToDefine = new List<string>();
        foreach (var type in types)
        {
            typesToDefine.Add(type.typeName);
        }
        // Create Empty list of defined types indexes(to help showing models in the inspector)
        typesDefined = new List<int>();
        // Create Empty list of game objects representing the models
        typesGameObject = new List<GameObject>();
    }

    public void Reset()
    {
        actions = null;
        predicates = null;
        types = null;
        typesDefined = null;
        typesToDefine = null;
        typesGameObject = null;
    }
}