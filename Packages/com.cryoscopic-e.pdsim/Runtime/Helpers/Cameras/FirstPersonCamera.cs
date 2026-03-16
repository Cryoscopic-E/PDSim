using UnityEngine;
using UnityEngine.InputSystem;

namespace PDSim.Helpers.Cameras
{
    /// <summary>
    /// First-person camera controller using the Unity Input System.
    ///
    /// Controls (when active):
    ///   Toggle on/off  — Right-click  or  C
    ///   Exit           — Escape
    ///   Look           — Mouse move
    ///   Move           — WASD / Arrow keys
    ///   Up / Down      — E / Q  (or Space / Left-Ctrl)
    ///   Sprint         — Left Shift  (multiplies move speed)
    /// </summary>
    public class FirstPersonCamera : MonoBehaviour
    {
        [Header("Look")]
        [Tooltip("Mouse sensitivity in degrees per pixel.")]
        public float lookSensitivity = 0.15f;

        [Tooltip("Vertical look clamp (degrees).")]
        public float pitchMin = -89f;
        public float pitchMax =  89f;

        [Header("Movement")]
        public float moveSpeed         = 8f;
        public float sprintMultiplier  = 2.5f;
        public float verticalSpeed     = 6f;

        [Header("Feel")]
        [Tooltip("Smoothing applied to movement (0 = instant, 1 = never moves).")]
        [Range(0f, 0.99f)]
        public float movementSmoothing = 0.1f;

        // ── State ────────────────────────────────────────────────────────────────

        private float   _yaw;
        private float   _pitch;
        private bool    _active;
        private Vector3 _velocity;

        // ── Lifecycle ────────────────────────────────────────────────────────────

        private void Awake()
        {
            // Initialise yaw/pitch from the transform so enabling doesn't snap
            Vector3 euler = transform.eulerAngles;
            _yaw   = euler.y;
            _pitch = euler.x > 180f ? euler.x - 360f : euler.x;
        }

        private void OnDisable() => SetActive(false);

        private void LateUpdate()
        {
            var keyboard = Keyboard.current;
            var mouse    = Mouse.current;
            if (keyboard == null || mouse == null) return;

            // ── Toggle ──────────────────────────────────────────────────────────
            if (keyboard.cKey.wasPressedThisFrame || mouse.rightButton.wasPressedThisFrame)
                SetActive(!_active);
            if (_active && keyboard.escapeKey.wasPressedThisFrame)
                SetActive(false);

            if (!_active) return;

            // ── Look ────────────────────────────────────────────────────────────
            Vector2 mouseDelta = mouse.delta.ReadValue();
            _yaw   += mouseDelta.x * lookSensitivity;
            _pitch -= mouseDelta.y * lookSensitivity;
            _pitch  = Mathf.Clamp(_pitch, pitchMin, pitchMax);

            // Yaw rotates the whole transform; pitch only tilts the view
            transform.rotation = Quaternion.Euler(_pitch, _yaw, 0f);

            // ── Movement input ──────────────────────────────────────────────────
            Vector3 wishDir = Vector3.zero;

            // Horizontal — yaw-aligned so pitch doesn't tilt movement
            Quaternion yawRot = Quaternion.Euler(0f, _yaw, 0f);

            if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed)
                wishDir += yawRot * Vector3.forward;
            if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed)
                wishDir += yawRot * Vector3.back;
            if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed)
                wishDir += yawRot * Vector3.right;
            if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed)
                wishDir += yawRot * Vector3.left;

            // Vertical — always world-space
            if (keyboard.eKey.isPressed || keyboard.spaceKey.isPressed)
                wishDir += Vector3.up;
            if (keyboard.qKey.isPressed || keyboard.leftCtrlKey.isPressed)
                wishDir += Vector3.down;

            if (wishDir.sqrMagnitude > 1f)
                wishDir.Normalize();

            float speed = moveSpeed;
            if (keyboard.leftShiftKey.isPressed) speed *= sprintMultiplier;

            Vector3 targetVelocity = wishDir * speed;

            // Smooth acceleration / deceleration
            float t = movementSmoothing <= 0f
                ? 1f
                : 1f - Mathf.Pow(movementSmoothing, Time.deltaTime * 60f);

            _velocity = Vector3.Lerp(_velocity, targetVelocity, t);
            transform.position += _velocity * Time.deltaTime;
        }

        // ── Helpers ──────────────────────────────────────────────────────────────

        private void SetActive(bool value)
        {
            _active           = value;
            Cursor.lockState  = value ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible    = !value;

            if (!value) _velocity = Vector3.zero;
        }
    }
}
