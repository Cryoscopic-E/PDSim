using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "Simulation Settings")]
public class SimulationSettings : ScriptableObject
{
    [SerializeField] public TextAsset domain;
    [FormerlySerializedAs("pddlElements")] [SerializeField] public DomainElements domainElements;
    [SerializeField] public GameObject[] typesModels;
    [SerializeField] public PredicateCommand[] predicatesBehaviours;

    public PredicateCommand GetPredicateBehaviour(string predicateName)
    {
        var indx = domainElements.predicates.FindIndex(a => a.name.Contains(predicateName));
        return predicatesBehaviours[indx];
    }

    public GameObject GetPrefabWithType(string typeName)
    {
        var indx = domainElements.types.FindIndex(a => a.Contains(typeName));
        return typesModels[indx];
    }
}                       