using GeTPlan.Core.Models.Expressions;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using PDSim.ScriptableObjects;

namespace PDSim.Components
{
    /// <summary>
    /// Represents an object in the planning domain within the Unity scene.
    /// Manages object-specific state, movement, and interaction with the planning system.
    /// </summary>
    public class VisualisationObject : MonoBehaviour
    {
        #region Public API

        /// <summary>
        /// The type of the object in the planning domain.
        /// </summary>
        [Header("Object Settings")]
        [Tooltip("The type of the object in the PDDL domain.")]
        public string ObjectType;

        /// <summary>
        /// Whether to use a NavMeshAgent for movement.
        /// </summary>
        [Tooltip("Use Navmesh Agent for movement.")]
        public bool UseNavMeshAgent = false;

        /// <summary>
        /// Optional movement settings for custom speed, acceleration, etc.
        /// </summary>
        [Tooltip("(Optional) Movement Settings")]
        public MovementSettings MovementSettings;

        /// <summary>
        /// Retrieves the current state of the object as a list of fluent assignments.
        /// </summary>
        /// <returns>A list of grounded fluents and their values.</returns>
        public List<(FluentExpression Fluent, object Value)> GetObjectState()
        {
            return _state.Values.ToList();
        }

        /// <summary>
        /// Adds a fluent assignment to the object's local state tracking.
        /// </summary>
        /// <param name="fluentAssignment">The grounded fluent and its value.</param>
        public void AddFluentAssignment((FluentExpression Fluent, object Value) fluentAssignment)
        {
            // Add only if object is active
            if (gameObject.activeSelf)
                _state[fluentAssignment.Fluent.Name] = fluentAssignment;
        }

        /// <summary>
        /// Smoothly moves the object to a target position.
        /// Uses NavMeshAgent if enabled, otherwise performs a manual Lerp.
        /// </summary>
        /// <param name="position">The target position in world space.</param>
        /// <param name="faceTarget">Whether the object should rotate to face the movement direction.</param>
        /// <returns>An enumerator for the movement coroutine.</returns>
        public IEnumerator MoveTo(Vector3 position, bool faceTarget = true)
        {
            if (UseNavMeshAgent)
            {
                if (MovementSettings != null)
                {
                    _navMeshAgent.speed = MovementSettings.Speed;
                    _navMeshAgent.angularSpeed = MovementSettings.AngularSpeed;
                    _navMeshAgent.acceleration = MovementSettings.Acceleration;
                    _navMeshAgent.stoppingDistance = MovementSettings.StoppingDistance;
                    _navMeshAgent.updateRotation = MovementSettings.FaceTarget;
                }
                else
                {
                    // Default values
                    _navMeshAgent.speed = Speed;
                    _navMeshAgent.angularSpeed = AngularSpeed;
                    _navMeshAgent.acceleration = Acceleration;
                    _navMeshAgent.stoppingDistance = StoppingDistance;
                    _navMeshAgent.updateRotation = faceTarget;
                }

                _navMeshAgent.SetDestination(position);

                while (_navMeshAgent.pathPending)
                {
                    yield return null;
                }

                yield return new WaitUntil(() => _navMeshAgent.remainingDistance <= _navMeshAgent.stoppingDistance);
            }
            else
            {
                // Lerp the object to the new position
                // Initialize variables from movement settings or defaults
                var t = 0f;
                var startPosition = transform.position;
                var targetPosition = position;
                var movementSettingsOrDefault = MovementSettings ?? new MovementSettings
                {
                    StoppingDistance = StoppingDistance,
                    Acceleration = Acceleration,
                    Speed = Speed,
                    FaceTarget = faceTarget,
                    AngularSpeed = AngularSpeed
                };

                // Use destructuring for cleaner access to settings
                var (stopDistance, acceleration, speed, focusTarget, angularSpeed) = (
                    movementSettingsOrDefault.StoppingDistance,
                    movementSettingsOrDefault.Acceleration,
                    movementSettingsOrDefault.Speed,
                    movementSettingsOrDefault.FaceTarget,
                    movementSettingsOrDefault.AngularSpeed
                );

                if (Vector3.Distance(transform.position, targetPosition) > movementSettingsOrDefault.StoppingDistance)
                {
                    while (Vector3.Distance(transform.position, targetPosition) > stopDistance)
                    {
                        t += Time.deltaTime * (speed / Vector3.Distance(startPosition, targetPosition)) * acceleration;
                        transform.position = Vector3.Lerp(startPosition, targetPosition, t);

                        if (focusTarget)
                        {
                            var targetDirection = (targetPosition - transform.position).normalized;
                            var targetRotation = Quaternion.LookRotation(targetDirection);
                            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * angularSpeed);
                        }

                        yield return null;
                    }
                }
            }
        }

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            _state = new Dictionary<string, (FluentExpression Fluent, object Value)>();

            if (!UseNavMeshAgent) return;
            _navMeshAgent = gameObject.GetComponent<NavMeshAgent>();
            if (_navMeshAgent == null)
            {
                _navMeshAgent = gameObject.AddComponent<NavMeshAgent>();
            }

            _navMeshAgent.enabled = UseNavMeshAgent;
        }

        private void OnMouseEnter()
        {
            ProblemObjects.Instance.HoverObject(this);
        }

        private void OnMouseExit()
        {
            ProblemObjects.Instance.ClearHover();
        }

        #endregion

        #region Private Internals

        private const float Speed = 1f;
        private const float AngularSpeed = 120f;
        private const float Acceleration = 8f;
        private const float StoppingDistance = 0.1f;

        private NavMeshAgent _navMeshAgent;
        private Dictionary<string, (FluentExpression Fluent, object Value)> _state;

        #endregion
    }
}