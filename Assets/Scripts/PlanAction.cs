using System.Collections.Generic;
using System.Linq;

[System.Serializable]
public struct PlanAction
{
    public string name;
    public List<string> parameters;


    public PlanAction(string name, List<string> parameters)
    {
        this.name = name;
        this.parameters = parameters;
    }
}