using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using SimpleJSON;

public class PlanSolver : MonoBehaviour
{
    const string URL = "solver.planning.domains/solve";
    public void GeneratePlan(string domain, string problem)
    {
        try
        {
            WWWForm www = new WWWForm();
            www.AddField("domain", domain);
            www.AddField("problem", problem);
            StartCoroutine(SendRequest(www));
        }
        catch (UnityException ue)
        {
            Debug.LogError(ue.Message);
        }
    }

    IEnumerator SendRequest(WWWForm form)
    {

        UnityWebRequest www = UnityWebRequest.Post(URL, form);
        yield return www.SendWebRequest();

        if (www.isNetworkError || www.isHttpError)
        {
            //Debug.Log(www.error);
            throw new UnityException("Network Error");
        }
        else
        {
            string response = www.downloadHandler.text;
            //Debug.Log(response);
            JSONNode res_json = JSON.Parse(response);
            string status = res_json["status"];
            int length = int.Parse(res_json["result"]["length"]);

            if (status.Equals("ok"))
            {
                //plan = new Plan(length, ActionParser(res_json, length));
                //PlanReady();
            }
        }
    }
}
