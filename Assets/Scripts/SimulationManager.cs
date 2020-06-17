using System;
using System.Collections;
using System.Collections.Generic;
using SimpleJSON;
using UnityEngine;
using UnityEngine.Networking;

public class SimulationManager : MonoBehaviour
{
    public SimulationSettings simulationSettings;
    public SimulationEnvironment simulationEnvironment;
    
    
    private Transform _objectsHolder;
    
    private Dictionary<string, GenericObject> objectsDictionary;
    private List<PlanAction> plan;
    private bool shouldRun = true;
    private bool started = false;
    private void Start()
    {
        if (shouldRun)
        {
            Plan();
            
            CheckObjectsHolder();
            // SETUP SIMULATION WITH SETTINGS
            SetupSimulation();
            // START SIMULATION ROUTINES
            
        }
    }

    private void Update()
    {
        if (plan != null && !started)
        {
            started = true;
            StartCoroutine(SimulatePlan());
        }
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

    public void Plan()
    {
        WWWForm www = new WWWForm();

        www.AddField("domain", simulationSettings.domain.text);
        www.AddField("problem", simulationEnvironment.problem.text);
        StartCoroutine(WaitForRequest(www));
    }

    IEnumerator WaitForRequest(WWWForm form)
    {

        UnityWebRequest www = UnityWebRequest.Post("solver.planning.domains/solve", form);
        yield return www.SendWebRequest();

        if (www.isNetworkError || www.isHttpError)
        {
            Debug.Log(www.error);
        }
        else
        {
            var actions = new List<PlanAction>();
            string response = www.downloadHandler.text;
            JSONNode planJson = JSON.Parse(response);
            string status = planJson["status"];
            int length = int.Parse(planJson["result"]["length"]);
            if (status.Equals("ok"))
            {
                var planLength = int.Parse(planJson["result"]["length"]);
                for (var i = 0; i < planLength; i++)
                {
                    string line = planJson["result"]["plan"][i]["name"];
                    // Remove parentheses
                    line = line.Remove(0, 1);
                    line = line.Remove(line.Length - 1, 1);
                    // Split spaces
                    var split = line.Split(' ');
                    // Action name is in first index
                    var methodName = split[0];

                    // Set action parameters
                    var parameters = new List<string>();

                    for (var j = 1; j < split.Length; j++)
                    {
                        parameters.Add(split[j]);
                    }

                    var action = new PlanAction(methodName, parameters) {parameters = parameters};
                    actions.Add(action);
                }
            }

            plan = actions;
        }
        
    }

    private IEnumerator SimulatePlan()
    {
        foreach (PlanAction action in plan)
        {
            var effects = simulationSettings.domainElements.GetEffectsOfAction(action.name);
            
            foreach (var predicate in effects)
            {
                // check action behaviour
                var behaviour = simulationSettings.GetPredicateBehaviour(predicate.predicateName);
                if (behaviour && !predicate.negate)
                {
                    List<GenericObject> param = GetObjects(action.parameters);
                    behaviour.SetAttributes(param);
                    yield return behaviour.Execute();
                }
            }
        }

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
        string[] objects = {"a", "b", "c", "d"};
        foreach (var obj in objects)
        {
            var gameObj = simulationSettings.GetPrefabWithType("block");
            Debug.Log(gameObj);
            var clone = Instantiate(gameObj, Vector3.zero,  Quaternion.identity, _objectsHolder) as GameObject;
            clone.name = obj;
        }
    }

}