using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;
using SimpleJSON;

public class PlanSolver : MonoBehaviour
{

    private const string Url = "http://solver.planning.domains/solve";

    public IEnumerator RequestPlan(string domain, string problem, Func<Plan,Plan> result)
    {
        var planActions = new List<PlanAction>();
        
        var www = new WWWForm();
        www.AddField("domain", domain);
        www.AddField("problem", problem);
        yield return WaitForRequest(www, value => planActions = value);
        result(new Plan(planActions));
    }

    IEnumerator WaitForRequest(WWWForm form, Func<List<PlanAction>,List<PlanAction>> result)
    {

        var www = UnityWebRequest.Post(Url, form);
        yield return www.SendWebRequest();

        if (www.isNetworkError || www.isHttpError) yield break;
        
        var response = www.downloadHandler.text;
        var planJson = JSON.Parse(response);
        string status = planJson["status"];
        if (status.Equals("ok"))
        {
            var l = Parser.ParsePlan(planJson);
            result(l);
        }

        yield return null;
    }
}
