using System;
using UnityEngine;
using System.Collections.Generic;

[Serializable]
public class ProblemElements
{
    [SerializeField] public List<PddlObject> objects;
    [SerializeField] public List<PddlInit> initBlock;
    
    public ProblemElements()
    {
        objects = new List<PddlObject>();
        initBlock = new List<PddlInit>();
    }
}
