using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Plan : ScriptableObject
{
    [SerializeField]
    public List<PlanAction> actions;
}
