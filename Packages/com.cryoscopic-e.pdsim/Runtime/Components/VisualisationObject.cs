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
    /// Visual feedback states applied by the <see cref="ObjectPicker"/>.
    /// </summary>
    public enum HighlightState
    {
        None,
        Hovered,
        Selected
    }

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
        /// Event fired whenever a fluent assignment changes this object's state.
        /// </summary>
        public event System.Action OnStateChanged;

        /// <summary>
        /// Adds a fluent assignment to the object's local state tracking.
        /// </summary>
        /// <param name="fluentAssignment">The grounded fluent and its value.</param>
        public void AddFluentAssignment((FluentExpression Fluent, object Value) fluentAssignment)
        {
            // Add only if object is active
            if (gameObject.activeSelf)
            {
                // Key by the full grounded fluent — the bare name would make
                // different groundings of the same predicate overwrite each other.
                _state[fluentAssignment.Fluent.ToString()] = fluentAssignment;
                OnStateChanged?.Invoke();
            }
        }

        /// <summary>
        /// Applies or clears the visual highlight tint for hover/selection feedback.
        /// Fluent animations (e.g. ColorAction) also write colors through
        /// MaterialPropertyBlocks, so the tint must blend with — and restore —
        /// whatever color is already applied instead of clobbering the block.
        /// </summary>
        /// <param name="state">The highlight state to apply.</param>
        public void SetHighlight(HighlightState state)
        {
            if (state == _highlightState) return;
            _highlightState = state;

            _renderers ??= GetComponentsInChildren<Renderer>(true);
            _propertyBlock ??= new MaterialPropertyBlock();
            _highlightBackup ??= new Dictionary<Renderer, HighlightBackup>();

            if (state == HighlightState.None)
            {
                RestoreHighlightColors();
                return;
            }

            var highlightColor = state == HighlightState.Selected ? SelectedTint : HoverTint;

            foreach (var renderer in _renderers)
            {
                if (renderer == null) continue;

                renderer.GetPropertyBlock(_propertyBlock);
                var hasBlockColor = TryReadBlockColor(_propertyBlock, out var currentColor);
                if (!hasBlockColor)
                    currentColor = GetMaterialColor(renderer);

                if (_highlightBackup.TryGetValue(renderer, out var backup))
                {
                    // An animation may have re-colored the object while it was
                    // highlighted — adopt the new color as the restore target.
                    if (hasBlockColor && currentColor != backup.AppliedTint)
                        backup = new HighlightBackup { HadBlockColor = true, OriginalColor = currentColor };
                }
                else
                {
                    backup = new HighlightBackup { HadBlockColor = hasBlockColor, OriginalColor = currentColor };
                }

                // Blend so the object keeps its own color identity under the tint.
                backup.AppliedTint = Color.Lerp(backup.OriginalColor, highlightColor, 0.5f);
                _highlightBackup[renderer] = backup;

                // Cover both built-in ("_Color") and URP/HDRP ("_BaseColor") shaders.
                _propertyBlock.SetColor(ColorId, backup.AppliedTint);
                _propertyBlock.SetColor(BaseColorId, backup.AppliedTint);
                renderer.SetPropertyBlock(_propertyBlock);
            }
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

        #endregion

        #region Private Internals

        private const float Speed = 1f;
        private const float AngularSpeed = 120f;
        private const float Acceleration = 8f;
        private const float StoppingDistance = 0.1f;

        private static readonly int ColorId = Shader.PropertyToID("_Color");
        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        private static readonly Color HoverTint = new Color(0.65f, 0.9f, 1f);
        private static readonly Color SelectedTint = new Color(1f, 0.85f, 0.45f);

        private struct HighlightBackup
        {
            public bool HadBlockColor;
            public Color OriginalColor;
            public Color AppliedTint;
        }

        private NavMeshAgent _navMeshAgent;
        private Renderer[] _renderers;
        private MaterialPropertyBlock _propertyBlock;
        private HighlightState _highlightState = HighlightState.None;
        private Dictionary<Renderer, HighlightBackup> _highlightBackup;
        private Dictionary<string, (FluentExpression Fluent, object Value)> _state;

        private static bool TryReadBlockColor(MaterialPropertyBlock block, out Color color)
        {
            if (block.HasColor(BaseColorId))
            {
                color = block.GetColor(BaseColorId);
                return true;
            }
            if (block.HasColor(ColorId))
            {
                color = block.GetColor(ColorId);
                return true;
            }
            color = Color.white;
            return false;
        }

        private static Color GetMaterialColor(Renderer renderer)
        {
            var material = renderer.sharedMaterial;
            if (material == null) return Color.white;
            if (material.HasProperty(BaseColorId)) return material.GetColor(BaseColorId);
            if (material.HasProperty(ColorId)) return material.GetColor(ColorId);
            return Color.white;
        }

        /// <summary>
        /// Puts back the pre-highlight colors. If an animation wrote a different
        /// color while the object was highlighted, that color wins and is kept.
        /// </summary>
        private void RestoreHighlightColors()
        {
            foreach (var renderer in _renderers)
            {
                if (renderer == null || !_highlightBackup.TryGetValue(renderer, out var backup))
                    continue;

                renderer.GetPropertyBlock(_propertyBlock);
                var hasBlockColor = TryReadBlockColor(_propertyBlock, out var currentColor);

                // Only restore when the block still holds our tint.
                if (!hasBlockColor || currentColor != backup.AppliedTint)
                    continue;

                if (backup.HadBlockColor)
                {
                    _propertyBlock.SetColor(ColorId, backup.OriginalColor);
                    _propertyBlock.SetColor(BaseColorId, backup.OriginalColor);
                    renderer.SetPropertyBlock(_propertyBlock);
                }
                else
                {
                    // The block only existed for the highlight — drop it entirely
                    // so the material's own color shows again.
                    renderer.SetPropertyBlock(null);
                }
            }

            _highlightBackup.Clear();
        }

        #endregion
    }
}