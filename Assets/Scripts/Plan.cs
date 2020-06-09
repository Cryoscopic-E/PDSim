using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Plan : ScriptableObject
{
    [SerializeField]
    public List<PlanAction> actions;
}
