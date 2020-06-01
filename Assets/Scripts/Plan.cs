using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Plan")]
public class Plan : ScriptableObject
{
    [SerializeField]
    public List<PlanAction> actions = new List<PlanAction>();
}
