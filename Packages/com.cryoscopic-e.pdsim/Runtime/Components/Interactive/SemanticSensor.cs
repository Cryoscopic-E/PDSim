using UnityEngine;
using System.Collections.Generic;
using PDSim.Runtime.Utils;
using GeTPlan.Core.Models.Expressions;

namespace PDSim.Components.Interactive
{
    /// <summary>
    /// Represents the sensing mode for the semantic sensor.
    /// </summary>
    public enum SensingMode
    {
        Raycast,
        Trigger
    }

    /// <summary>
    /// SemanticSensor senses objects in the environment and updates the world state fluents.
    /// Supports Raycast and Trigger modes for interaction.
    /// </summary>
    public class SemanticSensor : MonoBehaviour
    {
        #region Public API

        /// <summary>
        /// The sensing mode to use (Raycast or Trigger).
        /// </summary>
        [Header("Sensor Settings")]
        [SerializeField, Tooltip("The sensing mode to use.")]
        public SensingMode Mode = SensingMode.Raycast;

        /// <summary>
        /// The layer mask used to filter detectable objects.
        /// </summary>
        [SerializeField, Tooltip("The layer mask to filter detectable objects.")]
        public LayerMask LayerMask = -1;

        /// <summary>
        /// The DSL mapping expression used to translate a hit into a fluent change.
        /// Example: at[self, hit]
        /// </summary>
        [SerializeField, Tooltip("The DSL mapping expression (e.g., at[self, hit]).")]
        public string MappingExpression = "at[self, hit]";

        /// <summary>
        /// The maximum range for Raycast sensing.
        /// </summary>
        [SerializeField, Tooltip("The maximum range for Raycast sensing.")]
        public float DetectionRange = 10f;

        #endregion

        #region Unity Lifecycle

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
            if (Mode == SensingMode.Raycast)
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
            if (Mode == SensingMode.Raycast)
            {
                UpdateRaycast();
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (Mode != SensingMode.Trigger) return;
            if (IsValidTarget(other.gameObject))
            {
                if (_currentlySensed.Add(other.gameObject)) NotifyStateChange(other.gameObject, true);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (Mode != SensingMode.Trigger) return;
            if (IsValidTarget(other.gameObject))
            {
                if (_currentlySensed.Remove(other.gameObject)) NotifyStateChange(other.gameObject, false);
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (Mode == SensingMode.Raycast)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawLine(transform.position, transform.position + transform.forward * DetectionRange);
            }
        }

        #endregion

        #region Private Internals

        // Tracks objects currently within the sensor's trigger.
        private readonly HashSet<GameObject> _currentlySensed = new HashSet<GameObject>();

        // Tracks the last object hit by Raycast to detect changes.
        private GameObject _lastRaycastHit;

        private void UpdateRaycast()
        {
            RaycastHit hit;
            GameObject currentHit = null;

            if (Physics.Raycast(transform.position, transform.forward, out hit, DetectionRange, LayerMask))
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

        private bool IsValidTarget(GameObject go)
        {
            return go.GetComponent<ProblemObjectMetaData>() != null || go.GetComponent<VisualisationObject>() != null;
        }

        private void NotifyStateChange(GameObject target, object value)
        {
            if (string.IsNullOrEmpty(MappingExpression)) return;

            var expr = DslParser.Parse(MappingExpression, gameObject.name, target.name);
            if (expr is FluentExpression fluent && PDSimWorldObserver.Instance != null)
            {
                PDSimWorldObserver.Instance.UpdateFluent(fluent, value);
            }
        }

        #endregion
    }
}
