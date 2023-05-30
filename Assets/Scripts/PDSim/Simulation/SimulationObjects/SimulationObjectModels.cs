using Unity.VisualScripting;
using UnityEditor.SceneManagement;
using UnityEngine;


namespace PDSim.Simulation
{
    [ExecuteInEditMode]
    public class SimulationObjectModels : MonoBehaviour
    {
        private Variables _variables;

        private BoxCollider _bound;

        private void OnEnable()
        {
            _variables = GetComponentInParent<Variables>();
            _bound = GetComponentInParent<BoxCollider>();
            PrefabStage.prefabSaved += SaveModels;
        }

        private void OnDisable()
        {
            PrefabStage.prefabSaved -= SaveModels;
        }

        private void SaveModels(GameObject obj)
        {
            if (!transform.hasChanged) return;
            if (PrefabStageUtility.GetCurrentPrefabStage() is null) return;
            if (transform.childCount >= 0)
            {
                var modelDictionary = _variables.declarations.Get<AotDictionary>("Models");
                modelDictionary.Clear();
                foreach (Transform child in transform)
                {
                    var renderer = child.GetComponent<Renderer>();
                    if (renderer != null)
                    {
                        modelDictionary.Add(child.name + "-Model", renderer);
                    }
                }
                CalculateBounds();
            }
        }

        private void CalculateBounds()
        {
            var scenePosition = transform.position;

            // used to calculate bounds
            transform.position = Vector3.zero;
            var bound = new Bounds();
            var renderers = GetComponentsInChildren<Renderer>();

            foreach (var r in renderers)
            {
                bound.Encapsulate(r.bounds);
            }
            _bound.center = bound.center;
            _bound.size = bound.size;

            //reset old position in scene
            transform.position = scenePosition;
        }
    }
}
