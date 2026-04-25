using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

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
        #region Inspector Fields

        [Header("Look")]
        [Tooltip("Mouse sensitivity in degrees per pixel.")]
        [FormerlySerializedAs("lookSensitivity")]
        [SerializeField] private float LookSensitivity = 0.15f;

        [Tooltip("Vertical look clamp (degrees).")]
        [FormerlySerializedAs("pitchMin")]
        [SerializeField] private float PitchMin = -89f;

        [FormerlySerializedAs("pitchMax")]
        [SerializeField] private float PitchMax = 89f;

        [Header("Movement")]
        [FormerlySerializedAs("moveSpeed")]
        [SerializeField] private float MoveSpeed = 8f;

        [FormerlySerializedAs("sprintMultiplier")]
        [SerializeField] private float SprintMultiplier = 2.5f;

        [FormerlySerializedAs("verticalSpeed")]
        [SerializeField] private float VerticalSpeed = 6f;

        [Header("Feel")]
        [Tooltip("Smoothing applied to movement (0 = instant, 1 = never moves).")]
        [Range(0f, 0.99f)]
        [FormerlySerializedAs("movementSmoothing")]
        [SerializeField] private float MovementSmoothing = 0.1f;

        #endregion

        #region Private Internals

        private float _yaw;
        private float _pitch;
        private bool _active;
        private Vector3 _velocity;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            // Initialise yaw/pitch from the transform so enabling doesn't snap
            Vector3 euler = transform.eulerAngles;
            _yaw = euler.y;
            _pitch = euler.x > 180f ? euler.x - 360f : euler.x;
        }

        private void OnDisable() => SetActive(false);

        private void LateUpdate()
        {
            var keyboard = Keyboard.current;
            var mouse = Mouse.current;
            if (keyboard == null || mouse == null)
            {
                return;
            }

            // Toggle the first-person control state on user input.
            if (keyboard.cKey.wasPressedThisFrame || mouse.rightButton.wasPressedThisFrame)
            {
                SetActive(!_active);
            }
            if (_active && keyboard.escapeKey.wasPressedThisFrame)
            {
                SetActive(false);
            }

            if (!_active)
            {
                return;
            }

            // Process mouse movement to update the camera's orientation.
            Vector2 mouseDelta = mouse.delta.ReadValue();
            _yaw += mouseDelta.x * LookSensitivity;
            _pitch -= mouseDelta.y * LookSensitivity;
            _pitch = Mathf.Clamp(_pitch, PitchMin, PitchMax);

            // Yaw rotates the whole transform; pitch only tilts the view
            transform.rotation = Quaternion.Euler(_pitch, _yaw, 0f);

            // Calculate the movement direction based on keyboard input.
            Vector3 wishDir = Vector3.zero;

            // Horizontal — yaw-aligned so pitch doesn't tilt movement
            Quaternion yawRot = Quaternion.Euler(0f, _yaw, 0f);

            if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed)
            {
                wishDir += yawRot * Vector3.forward;
            }
            if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed)
            {
                wishDir += yawRot * Vector3.back;
            }
            if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed)
            {
                wishDir += yawRot * Vector3.right;
            }
            if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed)
            {
                wishDir += yawRot * Vector3.left;
            }

            // Vertical — always world-space
            if (keyboard.eKey.isPressed || keyboard.spaceKey.isPressed)
            {
                wishDir += Vector3.up;
            }
            if (keyboard.qKey.isPressed || keyboard.leftCtrlKey.isPressed)
            {
                wishDir += Vector3.down;
            }

            if (wishDir.sqrMagnitude > 1f)
            {
                wishDir.Normalize();
            }

            float speed = MoveSpeed;
            if (keyboard.leftShiftKey.isPressed)
            {
                speed *= SprintMultiplier;
            }

            Vector3 targetVelocity = wishDir * speed;

            // Smooth acceleration / deceleration
            float t = MovementSmoothing <= 0f
                ? 1f
                : 1f - Mathf.Pow(MovementSmoothing, Time.deltaTime * 60f);

            _velocity = Vector3.Lerp(_velocity, targetVelocity, t);
            transform.position += _velocity * Time.deltaTime;
        }

        #endregion

        #region Private Internals

        /// <summary>
        /// Sets the camera as active or inactive.
        /// When active, the cursor is locked and hidden.
        /// </summary>
        /// <param name="value">True to activate, false to deactivate.</param>
        private void SetActive(bool value)
        {
            _active = value;
            Cursor.lockState = value ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !value;

            if (!value)
            {
                _velocity = Vector3.zero;
            }
        }

        #endregion
    }
}
