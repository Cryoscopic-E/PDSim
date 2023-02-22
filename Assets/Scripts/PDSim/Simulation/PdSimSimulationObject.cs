using System.Collections;
using UnityEngine;
using UnityEngine.AI;

namespace PDSim.Simulation
{
    public class PdSimSimulationObject : MonoBehaviour
    {
        
        //PUBLIC VARIABLES

        [Tooltip("The name of the object in the PDDL domain.")]
        public string objectName;

        [Tooltip("The type of the object in the PDDL domain.")]
        public string objectType;
        
        [Tooltip("Use Navmesh Agent for movement.")]
        public bool useNavMeshAgent = false;
        
        public MovementSettings movementSettings;
    
        //PRIVATE VARIABLES
        // Default values
        private const float Speed = 1f;
        private const float AngularSpeed = 120f;
        private const float Acceleration = 8f;
        private const float StoppingDistance = 0.1f;
        
        private NavMeshAgent _navMeshAgent;
        
        private void Awake()
        {
            if (useNavMeshAgent)
            {
                _navMeshAgent = gameObject.AddComponent<NavMeshAgent>();
            }
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
                        objTransform.rotation = Quaternion.Slerp(objTransform.rotation, targetRotation, Time.deltaTime * angularSpeed);
                    }
                    
                    distance = Vector3.Distance(objTransform.position, position);
                    yield return null;
                }
            }
        }
        
        // Rotate the object to a new rotation
        public void RotateTo(Vector3 rotation)
        {
            transform.eulerAngles = rotation;
        }
        
        // Getters for world and local position, rotation, and scale
        public Vector3 GetWorldPosition()
        {
            return transform.position;
        }

        public Vector3 GetWorldRotation()
        {
            return transform.eulerAngles;
        }

        public Vector3 GetWorldScale()
        {
            return transform.localScale;
        }

        public Vector3 GetLocalPosition()
        {
            return transform.localPosition;
        }

        public Vector3 GetLocalRotation()
        {
            return transform.localEulerAngles;
        }

        public Vector3 GetLocalScale()
        {
            return transform.localScale;
        }

    #endregion HELPER FUNCTIONS
        
    }
}