using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "Simulation Environment")]
public class SimulationEnvironment : ScriptableObject
{
    [SerializeField] public TextAsset problem;
    [SerializeField] public ProblemElements problemElements;
    [SerializeField] public Plan plan;
}