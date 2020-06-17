using System.Collections.Generic;

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

    public string Parameters()
    {
        return string.Join(", ", parameters);
    }
}