using System.Collections.Generic;
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

        public List<Transform> points;


        public Transform GetPoint(string controlPoint)
        {
            var index = points.FindIndex(p => p.ToString() == controlPoint);
            if (index < 0)
            {
                return null;
            }

            return points[index];
        }

        private void OnEnable()
        {
            PrefabStage.prefabSaved += SaveControlPoints;
        }

        private void OnDisable()
        {
            PrefabStage.prefabSaved -= SaveControlPoints;
        }

        private void SaveControlPoints(GameObject obj)
        {
            if (!transform.hasChanged) return;
            if (PrefabStageUtility.GetCurrentPrefabStage() is null) return;
            if (transform.childCount >= 0)
            {
                if (points == null)
                    points = new List<Transform>();
                points.Clear();
                foreach (Transform child in transform)
                {
                    points.Add(child);
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

