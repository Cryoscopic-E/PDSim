using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace PDSim.Simulation
{
    [ExecuteInEditMode]
    public class SimulationObjectPoints : MonoBehaviour
    {
        private static readonly Color[] PointsGizmoColor =
        {
            Color.black, Color.blue, Color.cyan, Color.green, Color.green,
            Color.magenta, Color.red, Color.white, Color.yellow,
        };


        private Variables _variables;


        private void OnEnable()
        {
            _variables = GetComponentInParent<Variables>();
            //PrefabStage.prefabSaved += SaveControlPoints;
            PrefabStage.prefabStageDirtied += SaveControlPoints;
        }

        private void OnDisable()
        {
            //PrefabStage.prefabSaved -= SaveControlPoints;
            PrefabStage.prefabStageDirtied -= SaveControlPoints;
        }

        //private void SaveControlPoints(GameObject obj)
        private void SaveControlPoints(PrefabStage prefabStage)
        {
            if (!transform.hasChanged) return;
            if (transform.childCount >= 0)
            {
                _variables.declarations.Clear();
                foreach (Transform child in transform)
                {
                    _variables.declarations.Set(child.name + "-Point", child);
                }
            }
        }

        private void OnDrawGizmos()
        {
            if (PrefabStageUtility.GetCurrentPrefabStage() is null)
                return;
            for (var i = 0; i < transform.childCount; i++)
            {
                var child = transform.GetChild(i);
                Gizmos.color = PointsGizmoColor[i % PointsGizmoColor.Length];
                var position = child.position;
                Gizmos.DrawSphere(position, 0.05f);
                Handles.Label(position + Vector3.up * .2f, child.name);
            }
        }
    }
}

