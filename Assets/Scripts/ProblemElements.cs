using System.Collections.Generic;

[System.Serializable]
public class ProblemElements
{
    public List<PddlObject> objects;

    public ProblemElements()
    {
        objects = new List<PddlObject>();
    }
}