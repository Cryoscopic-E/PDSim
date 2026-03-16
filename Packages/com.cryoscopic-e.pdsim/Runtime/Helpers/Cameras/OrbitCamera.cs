using UnityEngine;

namespace PDSim.Helpers.Cameras
{
    /// <summary>
    /// Smooth orbit camera. Hold right-mouse-button and drag to rotate,
    /// scroll wheel to zoom, middle-mouse-button drag to pan.
    /// Assign a <see cref="target"/> Transform to orbit around, or leave it
    /// null to orbit around the initial look-at point.
    /// </summary>
    public class OrbitCamera : MonoBehaviour
    {
        [Header("Target")]
        [Tooltip("Transform to orbit around. Leave empty to use initialFocusPoint.")]
        public Transform target;

        [Tooltip("World-space focus point used when target is null.")]
        public Vector3 initialFocusPoint = Vector3.zero;

        [Header("Orbit")]
        public float orbitSpeed   = 150f;
        public float pitchMin     = -80f;
        public float pitchMax     =  80f;

        [Header("Zoom")]
        public float zoomSpeed    =  5f;
        public float minDistance  =  1f;
        public float maxDistance  = 50f;

        [Header("Pan")]
        public float panSpeed     =  0.3f;

        [Header("Smoothing")]
        [Range(0f, 1f)]
        public float smoothing    = 0.1f;

        // ── State ────────────────────────────────────────────────────────────────

        private float  _yaw;
        private float  _pitch   = 20f;
        private float  _distance;
        private Vector3 _focusOffset;   // pan displacement in focus-plane space

        private float  _targetYaw;
        private float  _targetPitch;
        private float  _targetDistance;

        private void Start()
        {
            Vector3 focus = FocusPoint;
            Vector3 toCamera = transform.position - focus;
            _distance        = toCamera.magnitude;
            _yaw             = Mathf.Atan2(toCamera.x, toCamera.z) * Mathf.Rad2Deg;
            _pitch           = Mathf.Asin(Mathf.Clamp01(toCamera.normalized.y)) * Mathf.Rad2Deg;

            _targetYaw      = _yaw;
            _targetPitch    = _pitch;
            _targetDistance = Mathf.Clamp(_distance, minDistance, maxDistance);
        }

        private void LateUpdate()
        {
            // ── Orbit ──────────────────────────────────────────────────────
            if (Input.GetMouseButton(1))
            {
                _targetYaw   += Input.GetAxisRaw("Mouse X") * orbitSpeed * Time.deltaTime;
                _targetPitch -= Input.GetAxisRaw("Mouse Y") * orbitSpeed * Time.deltaTime;
                _targetPitch  = Mathf.Clamp(_targetPitch, pitchMin, pitchMax);
            }

            // ── Zoom ───────────────────────────────────────────────────────
            float scroll = Input.GetAxisRaw("Mouse ScrollWheel");
            if (Mathf.Abs(scroll) > 0.001f)
                _targetDistance = Mathf.Clamp(_targetDistance - scroll * zoomSpeed, minDistance, maxDistance);

            // ── Pan ────────────────────────────────────────────────────────
            if (Input.GetMouseButton(2))
            {
                float panScale = _targetDistance * panSpeed * Time.deltaTime;
                _focusOffset -= transform.right   * Input.GetAxisRaw("Mouse X") * panScale;
                _focusOffset -= transform.up      * Input.GetAxisRaw("Mouse Y") * panScale;
            }

            // ── Smooth ─────────────────────────────────────────────────────
            float t = smoothing <= 0f ? 1f : 1f - Mathf.Pow(smoothing, Time.deltaTime * 60f);
            _yaw      = Mathf.LerpAngle(_yaw,      _targetYaw,      t);
            _pitch    = Mathf.LerpAngle(_pitch,    _targetPitch,    t);
            _distance = Mathf.Lerp(     _distance, _targetDistance, t);

            // ── Apply ──────────────────────────────────────────────────────
            Quaternion rot       = Quaternion.Euler(_pitch, _yaw, 0f);
            Vector3    focus     = FocusPoint + _focusOffset;
            transform.position   = focus + rot * new Vector3(0f, 0f, -_distance);
            transform.rotation   = rot;
        }

        private Vector3 FocusPoint =>
            target != null ? target.position : initialFocusPoint;

        /// <summary>Instantly snaps the camera to look at the given world-space point.</summary>
        public void FocusOn(Vector3 worldPoint)
        {
            _focusOffset    = Vector3.zero;
            initialFocusPoint = worldPoint;
            if (target != null) target = null;
        }
    }
}
