using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;
using PDSim.Components;

namespace PDSim.Helpers
{
    /// <summary>
    /// Briefly flashes a GameObject's renderer color to a highlight color and back.
    /// Useful for drawing attention to objects involved in the current plan action.
    ///
    /// Call <see cref="Flash"/> to trigger a single flash, or
    /// <see cref="Pulse"/> to loop until <see cref="StopPulse"/> is called.
    /// </summary>
    [RequireComponent(typeof(Renderer))]
    public class Helper_HighlightEffect : MonoBehaviour
    {
        #region Inspector Fields

        [Header("Settings")]
        [Tooltip("Color to flash to.")]
        [SerializeField]
        private Color HighlightColor = new Color(1f, 0.9f, 0f, 1f); // yellow

        [Tooltip("Total duration of one flash cycle (seconds).")]
        [SerializeField]
        private float FlashDuration = 0.35f;

        [Tooltip("Number of full flash cycles for a single Flash() call.")]
        [SerializeField]
        [Min(1)]
        private int PulseCount = 2;

        #endregion

        #region State

        private Renderer _renderer;
        private MaterialPropertyBlock _block;
        private Color _originalColor;
        private Coroutine _activeRoutine;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            _renderer = GetComponent<Renderer>();
            _block = new MaterialPropertyBlock();
        }

        private void Start()
        {
            _renderer.GetPropertyBlock(_block);
            _originalColor = _renderer.material.color;
        }

        #endregion

        #region Public API

        /// <summary>
        /// Plays <see cref="PulseCount"/> flash cycles then restores the original color.
        /// If called while already flashing, restarts the effect.
        /// </summary>
        public void Flash()
        {
            if (_activeRoutine != null) StopCoroutine(_activeRoutine);
            _activeRoutine = StartCoroutine(FlashRoutine(PulseCount, false));
        }

        /// <summary>
        /// Starts continuous pulsing. Call <see cref="StopPulse"/> to stop.
        /// </summary>
        public void Pulse()
        {
            if (_activeRoutine != null) StopCoroutine(_activeRoutine);
            _activeRoutine = StartCoroutine(FlashRoutine(int.MaxValue, true));
        }

        /// <summary>Stops pulsing and restores the original color immediately.</summary>
        public void StopPulse()
        {
            if (_activeRoutine != null)
            {
                StopCoroutine(_activeRoutine);
                _activeRoutine = null;
            }
            SetColor(_originalColor);
        }

        #endregion

        #region Implementation

        private IEnumerator FlashRoutine(int cycles, bool loop)
        {
            _renderer.GetPropertyBlock(_block);
            _originalColor = _renderer.material.color;

            for (int i = 0; i < cycles; i++)
            {
                // Fade to highlight
                yield return TweenColor(_originalColor, HighlightColor, FlashDuration * 0.5f);
                // Fade back
                yield return TweenColor(HighlightColor, _originalColor, FlashDuration * 0.5f);

                if (!loop && i == cycles - 1) break;
            }

            SetColor(_originalColor);
            _activeRoutine = null;
        }

        private IEnumerator TweenColor(Color from, Color to, float duration)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                // Pause-aware: respect Controller animation speed
                if (Controller.Instance != null && Controller.Instance.IsPaused)
                {
                    yield return null;
                    continue;
                }
                float speed = Controller.Instance != null ? Controller.Instance.AnimationSpeed : 1f;
                elapsed += Time.deltaTime * speed;
                SetColor(Color.Lerp(from, to, Mathf.Clamp01(elapsed / duration)));
                yield return null;
            }
            SetColor(to);
        }

        private void SetColor(Color c)
        {
            _block.SetColor("_Color", c);
            _block.SetColor("_BaseColor", c); // URP
            _renderer.SetPropertyBlock(_block);
        }

        #endregion
    }
}
