using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "Simulation Settings")]
public class SimulationSettings : ScriptableObject
{
    [SerializeField] public TextAsset domain;
    [FormerlySerializedAs("pddlElements")] [SerializeField] public DomainElements domainElements;
    [SerializeField] public GameObject[] typesModels;
}                       