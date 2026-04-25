using UnityEngine;
using UnityEngine.Serialization;

namespace PDSim.Helpers.Cameras
{
    /// <summary>
    /// Smooth orbit camera. Hold right-mouse-button and drag to rotate,
    /// scroll wheel to zoom, middle-mouse-button drag to pan.
    /// Assign a <see cref="Target"/> Transform to orbit around, or leave it
    /// null to orbit around the initial look-at point.
    /// </summary>
    public class OrbitCamera : MonoBehaviour
    {
        #region Inspector Fields

        [Header("Target")]
        [Tooltip("Transform to orbit around. Leave empty to use initialFocusPoint.")]
        [FormerlySerializedAs("target")]
        [SerializeField] private Transform _target;

        [Tooltip("World-space focus point used when target is null.")]
        [FormerlySerializedAs("initialFocusPoint")]
        [SerializeField] private Vector3 _initialFocusPoint = Vector3.zero;

        [Header("Orbit")]
        [FormerlySerializedAs("orbitSpeed")]
        [SerializeField] private float OrbitSpeed = 150f;

        [FormerlySerializedAs("pitchMin")]
        [SerializeField] private float PitchMin = -80f;

        [FormerlySerializedAs("pitchMax")]
        [SerializeField] private float PitchMax = 80f;

        [Header("Zoom")]
        [FormerlySerializedAs("zoomSpeed")]
        [SerializeField] private float ZoomSpeed = 5f;

        [FormerlySerializedAs("minDistance")]
        [SerializeField] private float MinDistance = 1f;

        [FormerlySerializedAs("maxDistance")]
        [SerializeField] private float MaxDistance = 50f;

        [Header("Pan")]
        [FormerlySerializedAs("panSpeed")]
        [SerializeField] private float PanSpeed = 0.3f;

        [Header("Smoothing")]
        [Range(0f, 1f)]
        [FormerlySerializedAs("smoothing")]
        [SerializeField] private float Smoothing = 0.1f;

        #endregion

        #region Private Internals

        private float _yaw;
        private float _pitch = 20f;
        private float _distance;
        private Vector3 _focusOffset; // pan displacement in focus-plane space

        private float _targetYaw;
        private float _targetPitch;
        private float _targetDistance;

        /// <summary>
        /// Gets the current focus point of the camera.
        /// </summary>
        private Vector3 FocusPoint =>
            _target != null ? _target.position : _initialFocusPoint;

        #endregion

        #region Unity Lifecycle

        private void Start()
        {
            Vector3 focus = FocusPoint;
            Vector3 toCamera = transform.position - focus;
            _distance = toCamera.magnitude;
            _yaw = Mathf.Atan2(toCamera.x, toCamera.z) * Mathf.Rad2Deg;
            _pitch = Mathf.Asin(Mathf.Clamp01(toCamera.normalized.y)) * Mathf.Rad2Deg;

            _targetYaw = _yaw;
            _targetPitch = _pitch;
            _targetDistance = Mathf.Clamp(_distance, MinDistance, MaxDistance);
        }

        private void LateUpdate()
        {
            // Handle orbit rotation input when the right mouse button is held.
            if (Input.GetMouseButton(1))
            {
                _targetYaw += Input.GetAxisRaw("Mouse X") * OrbitSpeed * Time.deltaTime;
                _targetPitch -= Input.GetAxisRaw("Mouse Y") * OrbitSpeed * Time.deltaTime;
                _targetPitch = Mathf.Clamp(_targetPitch, PitchMin, PitchMax);
            }

            // Handle zoom input using the mouse scroll wheel.
            float scroll = Input.GetAxisRaw("Mouse ScrollWheel");
            if (Mathf.Abs(scroll) > 0.001f)
            {
                _targetDistance = Mathf.Clamp(_targetDistance - scroll * ZoomSpeed, MinDistance, MaxDistance);
            }

            // Handle panning input when the middle mouse button is held.
            if (Input.GetMouseButton(2))
            {
                float panScale = _targetDistance * PanSpeed * Time.deltaTime;
                _focusOffset -= transform.right * Input.GetAxisRaw("Mouse X") * panScale;
                _focusOffset -= transform.up * Input.GetAxisRaw("Mouse Y") * panScale;
            }

            // Interpolate towards the target values for smooth camera movement.
            float t = Smoothing <= 0f ? 1f : 1f - Mathf.Pow(Smoothing, Time.deltaTime * 60f);
            _yaw = Mathf.LerpAngle(_yaw, _targetYaw, t);
            _pitch = Mathf.LerpAngle(_pitch, _targetPitch, t);
            _distance = Mathf.Lerp(_distance, _targetDistance, t);

            // Calculate and apply the final camera position and rotation.
            Quaternion rot = Quaternion.Euler(_pitch, _yaw, 0f);
            Vector3 focus = FocusPoint + _focusOffset;
            transform.position = focus + rot * new Vector3(0f, 0f, -_distance);
            transform.rotation = rot;
        }

        #endregion

        #region Public API

        /// <summary>
        /// Instantly snaps the camera to look at the given world-space point.
        /// </summary>
        /// <param name="worldPoint">The point to focus on.</param>
        public void FocusOn(Vector3 worldPoint)
        {
            _focusOffset = Vector3.zero;
            _initialFocusPoint = worldPoint;
            if (_target != null)
            {
                _target = null;
            }
        }

        #endregion
    }
}
