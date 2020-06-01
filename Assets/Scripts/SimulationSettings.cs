using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CreateAssetMenu(fileName = "Simulation Settings")]
public class SimulationSettings : ScriptableObject
{
    [SerializeField] public TextAsset domain;
    [SerializeField] public TextAsset problem;
    [SerializeField] public PddlElements pddlElements;
    [SerializeField] public GameObject[] typesModels;
}