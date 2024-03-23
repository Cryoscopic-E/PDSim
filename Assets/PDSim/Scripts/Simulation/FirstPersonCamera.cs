using UnityEngine;

namespace PDSim.Simulation
{


    public class FirstPersonCamera : MonoBehaviour
    {
        public float moveSpeed = 20.0f;
        public float lookSpeed = 55.0f;

        private Vector2 _rotation = Vector2.zero;

        private float _delta;
        private float _previousTime;

        private bool controlEnabled = false;

        public void Awake()
        {
            _previousTime = Time.realtimeSinceStartup;
        }

        public void LateUpdate()
        {
            /* Calculate delta time to be time indipendent*/
            var currentTime = Time.realtimeSinceStartup;
            _delta = currentTime - _previousTime;
            _previousTime = currentTime;
            if (_delta < 0)
            {
                _delta = 0;
            }

            if (Input.GetKeyDown(KeyCode.C))
            {
                controlEnabled = !controlEnabled;
            }

            if (!controlEnabled) return;
            _rotation.x += Input.GetAxis("Mouse X") * lookSpeed;
            _rotation.y += Input.GetAxis("Mouse Y") * lookSpeed;
            _rotation.y = Mathf.Clamp(_rotation.y, -90, 90);

            transform.localRotation = Quaternion.AngleAxis(_rotation.x, Vector3.up);
            transform.localRotation *= Quaternion.AngleAxis(_rotation.y, Vector3.left);

            transform.position += transform.forward * moveSpeed * Input.GetAxisRaw("Vertical") * _delta;
            transform.position += transform.right * moveSpeed * Input.GetAxisRaw("Horizontal") * _delta;
        }
    }
}