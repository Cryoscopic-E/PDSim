using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

[ExecuteInEditMode]
public class SimulationObjectComponents : MonoBehaviour
{
    private static readonly Color[] PointsGizmoColor =
    {
        Color.black, Color.blue, Color.cyan, Color.green, Color.green,
        Color.magenta, Color.red, Color.white, Color.yellow,
    };

    private Variables _variables;
    private Transform _points;
    private Transform _models;
    private BoxCollider _bound;

    private void OnEnable()
    {
        _variables = GetComponentInParent<Variables>();
        PrefabStage.prefabSaved += SaveComponents;

        // Check if Points and Modles objects exist or create
        _points = transform.Find("Points");
        _models = transform.Find("Models");
        if (_points == null)
        {
            _points = new GameObject("Points").transform;
            _points.parent = transform;
        }

        if (_models == null)
        {
            _models = new GameObject("Models").transform;
            _models.parent = transform;
        }

        // Check if Bound object exists or create
        _bound = GetComponent<BoxCollider>();
        if (_bound == null)
        {
            _bound = gameObject.AddComponent<BoxCollider>();
        }

    }

    private void OnDisable()
    {
        PrefabStage.prefabSaved -= SaveComponents;

    }


    private void SaveComponents(GameObject gameObject)
    {
        if (PrefabStageUtility.GetCurrentPrefabStage() is null) return;
        if (!_points.hasChanged || !_models.hasChanged) return;
        _variables.declarations.Clear();
        if (_points.childCount >= 0)
        {
            foreach (Transform child in _points)
            {
                _variables.declarations.Set(child.name + "-Point", child);
            }
        }

        if (_models.childCount >= 0)
        {
            foreach (Transform child in _models)
            {
                var renderer = child.GetComponent<Renderer>();
                if (renderer != null)
                {
                    _variables.declarations.Set(child.name + "-Model", renderer);
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

    private void OnDrawGizmos()
    {
        if (PrefabStageUtility.GetCurrentPrefabStage() is null)
            return;
        for (var i = 0; i < _points.childCount; i++)
        {
            var child = _points.GetChild(i);
            Gizmos.color = PointsGizmoColor[i % PointsGizmoColor.Length];
            var position = child.position;
            Gizmos.DrawSphere(position, 0.05f);
            Handles.Label(position + Vector3.up * .2f, child.name);
        }
    }

}
