using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using GeTPlan.Core.Models.Expressions;
using PDSim.Components;
using PDSim.Runtime.Utils;

namespace PDSim.Interactive
{
    public enum SensingMode
    {
        Raycast,
        Trigger
    }

    /// <summary>
    /// SemanticSensor senses objects in the environment and updates the world state fluents.
    /// Supports Raycast and Trigger modes.
    /// </summary>
    public class SemanticSensor : MonoBehaviour
    {
        [Header("Sensor Settings")]
        [SerializeField, Tooltip("The sensing mode to use.")]
        private SensingMode mode = SensingMode.Raycast;

        [SerializeField, Tooltip("The layer mask to filter detectable objects.")]
        private LayerMask layerMask = -1;

        [SerializeField, Tooltip("The DSL mapping expression (e.g., at[self, hit]).")]
        private string mappingExpression = "at[self, hit]";

        [SerializeField, Tooltip("The maximum range for Raycast sensing.")]
        private float detectionRange = 10f;

        // Tracks objects currently within the sensor's trigger.
        private readonly HashSet<GameObject> _currentlySensed = new HashSet<GameObject>();

        // Tracks the last object hit by Raycast to detect changes.
        private GameObject _lastRaycastHit;

        private void OnEnable()
        {
            if (PDSimWorldObserver.Instance != null)
            {
                PDSimWorldObserver.Instance.RegisterSensor(this);
            }
        }

        private void OnDisable()
        {
            if (PDSimWorldObserver.Instance != null)
            {
                PDSimWorldObserver.Instance.UnregisterSensor(this);
            }

            // Revert all currently sensed objects when sensor is disabled
            if (mode == SensingMode.Raycast)
            {
                if (_lastRaycastHit != null)
                {
                    NotifyStateChange(_lastRaycastHit, false);
                    _lastRaycastHit = null;
                }
            }
            else
            {
                foreach (var go in _currentlySensed)
                {
                    NotifyStateChange(go, false);
                }
                _currentlySensed.Clear();
            }
        }

        private void FixedUpdate()
        {
            if (mode == SensingMode.Raycast)
            {
                UpdateRaycast();
            }
        }

        private void UpdateRaycast()
        {
            RaycastHit hit;
            GameObject currentHit = null;

            if (Physics.Raycast(transform.position, transform.forward, out hit, detectionRange, layerMask))
            {
                if (IsValidTarget(hit.collider.gameObject))
                {
                    currentHit = hit.collider.gameObject;
                }
            }

            if (currentHit != _lastRaycastHit)
            {
                if (_lastRaycastHit != null) NotifyStateChange(_lastRaycastHit, false);
                if (currentHit != null) NotifyStateChange(currentHit, true);
                _lastRaycastHit = currentHit;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (mode != SensingMode.Trigger) return;
            if (IsValidTarget(other.gameObject))
            {
                if (_currentlySensed.Add(other.gameObject)) NotifyStateChange(other.gameObject, true);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (mode != SensingMode.Trigger) return;
            if (IsValidTarget(other.gameObject))
            {
                if (_currentlySensed.Remove(other.gameObject)) NotifyStateChange(other.gameObject, false);
            }
        }

        private bool IsValidTarget(GameObject go)
        {
            return go.GetComponent<PDSimMetadata>() != null || go.GetComponent<VisualisationObject>() != null;
        }

        private void NotifyStateChange(GameObject target, object value)
        {
            if (string.IsNullOrEmpty(mappingExpression)) return;

            var expr = DslParser.Parse(mappingExpression, gameObject.name, target.name);
            if (expr is FluentExpression fluent && PDSimWorldObserver.Instance != null)
            {
                PDSimWorldObserver.Instance.UpdateFluent(fluent, value);
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (mode == SensingMode.Raycast)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawLine(transform.position, transform.position + transform.forward * detectionRange);
            }
        }
    }
}
