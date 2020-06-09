using System.Net.Http;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using SimpleJSON;

public class PlanSolver 
{

    private static PlanSolver _instance;
    public static PlanSolver Instance => _instance ?? (_instance = new PlanSolver());
    
    private const string Url = "http://solver.planning.domains/solve";
    private static readonly HttpClient HttpClient = new HttpClient();
    
    private PlanSolver()
    {
    }

    public async Task<List<PlanAction>> GeneratePlan(string domain, string problem)
    {
        var content = new Dictionary<string, string>()
        {
            {"domain", domain},
            {"problem", problem}
        };
        
        var form = new FormUrlEncodedContent(content);
        var res = await HttpClient.PostAsync(Url, form);
        var planObj = await res.Content.ReadAsStringAsync();
        var planJson = JSON.Parse(planObj);
        var actions = new List<PlanAction>();
        string status = planJson["status"];
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
        return actions;
    }
}
