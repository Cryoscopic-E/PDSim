using PDSim.Protobuf;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

namespace PDSim.Simulation
{
    public class PdSimSimulationObject : MonoBehaviour
    {
        // Object settings
        // ---------------

        [Tooltip("The type of the object in the PDDL domain.")]
        public string objectType;

        [Tooltip("Use Navmesh Agent for movement.")]
        public bool useNavMeshAgent = false;

        [Tooltip("(Optional) Movement Settings")]
        public MovementSettings movementSettings;


        // Movement settings
        // -----------------

        private const float Speed = 1f;
        private const float AngularSpeed = 120f;
        private const float Acceleration = 8f;
        private const float StoppingDistance = 0.1f;

        // NavMeshAgent 
        // ------------

        private NavMeshAgent _navMeshAgent;

        // Object State
        // ------------

        // Keep track of the object's state when actions are applied
        private Dictionary<string, PdSimFluentAssignment> state;

        public List<PdSimFluentAssignment> GetObjectState()
        {
            return state.Values.ToList();
        }

        // Add a fluent assignment to the object's state
        public void AddFluentAssignment(PdSimFluentAssignment fluentAssignment)
        {
            // Add only if object is active
            if (gameObject.activeSelf)
                state[fluentAssignment.fluentName] = fluentAssignment;
        }

        private void Awake()
        {
            state = new Dictionary<string, PdSimFluentAssignment>();

            if (!useNavMeshAgent) return;
            _navMeshAgent = gameObject.GetComponent<NavMeshAgent>();
            if (_navMeshAgent == null)
            {
                _navMeshAgent = gameObject.AddComponent<NavMeshAgent>();
            }

            _navMeshAgent.enabled = useNavMeshAgent;
        }

        private void OnMouseEnter()
        {
            PdSimManager.Instance.HoverObject(this);
        }

        #region HELPER FUNCTIONS

        // Move the object to a new position
        public IEnumerator MoveTo(Vector3 position, bool faceTarget = true)
        {
            if (useNavMeshAgent)
            {
                if (movementSettings != null)
                {
                    _navMeshAgent.speed = movementSettings.speed;
                    _navMeshAgent.angularSpeed = movementSettings.angularSpeed;
                    _navMeshAgent.acceleration = movementSettings.acceleration;
                    _navMeshAgent.stoppingDistance = movementSettings.stoppingDistance;
                    _navMeshAgent.updateRotation = movementSettings.faceTarget;
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
                var targetPosition = position; // Assuming 'position' is a parameter or a known variable
                var movementSettingsOrDefault = movementSettings ?? new MovementSettings
                {
                    stoppingDistance = StoppingDistance,
                    acceleration = Acceleration,
                    speed = Speed,
                    faceTarget = faceTarget,
                    angularSpeed = AngularSpeed
                };

                // Use destructuring for cleaner access to settings
                var (stopDistance, acceleration, speed, focusTarget, angularSpeed) = (
                    movementSettingsOrDefault.stoppingDistance,
                    movementSettingsOrDefault.acceleration,
                    movementSettingsOrDefault.speed,
                    movementSettingsOrDefault.faceTarget,
                    movementSettingsOrDefault.angularSpeed
                );

                if (Vector3.Distance(transform.position, targetPosition) > movementSettingsOrDefault.stoppingDistance)
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

        #endregion HELPER FUNCTIONS


    }
}