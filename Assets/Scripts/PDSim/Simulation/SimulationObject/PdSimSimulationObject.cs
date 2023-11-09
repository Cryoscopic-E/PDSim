using PDSim.Protobuf;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

namespace PDSim.Simulation.SimulationObject
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

        private void OnMouseExit()
        {
            PdSimManager.Instance.ClearHover();
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
            }
            else
            {
                // Lerp the object to the new position
                var t = 0f;
                var startPosition = transform.position;
                var distance = Vector3.Distance(startPosition, position);
                var stopDistance = movementSettings != null ? movementSettings.stoppingDistance : StoppingDistance;
                var acceleration = movementSettings != null ? movementSettings.acceleration : Acceleration;
                var speed = movementSettings != null ? movementSettings.speed : Speed;
                var focusTarget = movementSettings != null ? movementSettings.faceTarget : faceTarget;
                var angularSpeed = movementSettings != null ? movementSettings.angularSpeed : AngularSpeed;
                var objTransform = transform;
                while (distance > stopDistance)
                {
                    t += Time.deltaTime * (speed / distance) * acceleration;
                    transform.position = Vector3.Lerp(startPosition, position, t);

                    if (focusTarget)
                    {
                        var targetRotation = Quaternion.LookRotation(position - objTransform.position);
                        objTransform.rotation = Quaternion.Slerp(objTransform.rotation, targetRotation,
                            Time.deltaTime * angularSpeed);
                    }

                    distance = Vector3.Distance(objTransform.position, position);
                    yield return null;
                }
            }
        }

        #endregion HELPER FUNCTIONS


    }
}