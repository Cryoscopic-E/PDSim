using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Simulation Environment")]
public class SimulationEnvironment : ScriptableObject
{
    [SerializeField] public TextAsset problem;
    [SerializeField] public ProblemElements problemElements;
    [SerializeField] public Vector3[] objectsPositions;
    [SerializeField] public Plan plan;
}