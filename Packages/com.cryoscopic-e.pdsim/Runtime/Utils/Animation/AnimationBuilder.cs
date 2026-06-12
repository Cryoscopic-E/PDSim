using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using UnityEngine.UIElements;
using PDSim.Components;

namespace PDSim.Utils.Animation
{
    /// <summary>
    /// Represents a single animation step that can be executed as a coroutine.
    /// </summary>
    public interface IAnimationAction
    {
        /// <summary>
        /// Executes the animation action.
        /// </summary>
        /// <returns>An enumerator for the animation coroutine.</returns>
        IEnumerator Execute();
    }

    /// <summary>
    /// Fluent interface for building animation sequences.
    /// </summary>
    public interface IAnimationBuilder
    {
        /// <summary>Moves a GameObject to a destination.</summary>
        ITransformationBuilder Move(GameObject target);
        /// <summary>Moves a GameObject using a specific anchor point.</summary>
        ITransformationBuilder Move(GameObject target, string anchorTag);
        /// <summary>Rotates a GameObject.</summary>
        ITransformationBuilder Rotate(GameObject target);
        /// <summary>Rotates a GameObject using a specific anchor point.</summary>
        ITransformationBuilder Rotate(GameObject target, string anchorTag);
        /// <summary>Scales a GameObject.</summary>
        ITransformationBuilder Scale(GameObject target);
        /// <summary>Shows a GameObject.</summary>
        IAnimationBuilder Show(GameObject target);
        /// <summary>Hides a GameObject.</summary>
        IAnimationBuilder Hide(GameObject target);
        /// <summary>Changes the color of a GameObject's renderer.</summary>
        IAnimationBuilder Color(GameObject target, Color color);
        /// <summary>Changes the color of a specific aesthetic part of a GameObject.</summary>
        IAnimationBuilder Color(GameObject target, string aestheticTag, Color color);
        /// <summary>Updates text on a GameObject's UI or TMP component.</summary>
        IAnimationBuilder Text(GameObject target, string displayTag, string text);
        /// <summary>Attaches a child GameObject to a parent.</summary>
        IAnimationBuilder Attach(GameObject child, GameObject parent);
        /// <summary>Detaches a child GameObject from its parent.</summary>
        IAnimationBuilder Detach(GameObject child);
        /// <summary>Waits for a specified number of seconds.</summary>
        IAnimationBuilder Wait(float seconds);
        /// <summary>Defines the next step in a sequential animation.</summary>
        IAnimationBuilder Then();
        /// <summary>Starts a block of animations that run in parallel.</summary>
        IAnimationBuilder Parallel();
        /// <summary>Ends a parallel block or the animation sequence.</summary>
        IAnimationBuilder End();
        /// <summary>Returns an enumerator to play the built animation.</summary>
        IEnumerator Play();
    }

    /// <summary>
    /// Fluent interface for building transformations (move, rotate, scale).
    /// </summary>
    public interface ITransformationBuilder : IAnimationBuilder
    {
        /// <summary>Sets the destination position.</summary>
        ITransformationBuilder To(Vector3 position);
        /// <summary>Sets the destination target GameObject.</summary>
        ITransformationBuilder To(GameObject target);
        /// <summary>Sets the destination target GameObject and anchor tag.</summary>
        ITransformationBuilder To(GameObject target, string anchorTag);
        /// <summary>Sets a relative movement or rotation delta.</summary>
        ITransformationBuilder By(Vector3 axis);
        /// <summary>Sets the destination rotation.</summary>
        ITransformationBuilder To(Quaternion rotation);
        /// <summary>Sets the duration of the transformation.</summary>
        ITransformationBuilder Duration(float seconds);
        /// <summary>Sets the easing function for the transformation.</summary>
        ITransformationBuilder WithEasing(EasingType type);
        /// <summary>Specifies that the transformation should occur in local space.</summary>
        ITransformationBuilder InLocalSpace();
    }

    /// <summary>
    /// Implementation of the fluent animation builder.
    /// </summary>
    public class AnimationBuilder : ITransformationBuilder
    {
        internal readonly List<IAnimationAction> Actions = new List<IAnimationAction>();
        private readonly bool _isParallel;
        private readonly AnimationBuilder _parent;

        public AnimationBuilder(bool isParallel = false, AnimationBuilder parent = null)
        {
            _isParallel = isParallel;
            _parent = parent;
        }

        /// <inheritdoc/>
        public ITransformationBuilder Move(GameObject target) => AddAction(new MoveAction(target));
        /// <inheritdoc/>
        public ITransformationBuilder Move(GameObject target, string anchorTag) => AddAction(new MoveAction(target, anchorTag));
        /// <inheritdoc/>
        public ITransformationBuilder Rotate(GameObject target) => AddAction(new RotateAction(target));
        /// <inheritdoc/>
        public ITransformationBuilder Rotate(GameObject target, string anchorTag) => AddAction(new RotateAction(target, anchorTag));
        /// <inheritdoc/>
        public ITransformationBuilder Scale(GameObject target) => AddAction(new ScaleAction(target));
        /// <inheritdoc/>
        public IAnimationBuilder Show(GameObject target) => AddAction(new InstantAction(() => target?.SetActive(true)));
        /// <inheritdoc/>
        public IAnimationBuilder Hide(GameObject target) => AddAction(new InstantAction(() => target?.SetActive(false)));
        /// <inheritdoc/>
        public IAnimationBuilder Color(GameObject target, Color color) => AddAction(new ColorAction(target, color));
        /// <inheritdoc/>
        public IAnimationBuilder Color(GameObject target, string aestheticTag, Color color) => AddAction(new ColorAction(target, aestheticTag, color));
        /// <inheritdoc/>
        public IAnimationBuilder Text(GameObject target, string displayTag, string text) => AddAction(new TextAction(target, displayTag, text));
        /// <inheritdoc/>
        public IAnimationBuilder Attach(GameObject child, GameObject parent) => AddAction(new InstantAction(() => child?.transform.SetParent(parent?.transform, true)));
        /// <inheritdoc/>
        public IAnimationBuilder Detach(GameObject child) => AddAction(new InstantAction(() => child?.transform.SetParent(null, true)));
        /// <inheritdoc/>
        public IAnimationBuilder Wait(float seconds) => AddAction(new WaitAction(seconds));

        /// <inheritdoc/>
        public IAnimationBuilder Then() => this;

        /// <inheritdoc/>
        public IAnimationBuilder Parallel()
        {
            var parallel = new ParallelBlock(this);
            Actions.Add(parallel);
            return parallel.InternalBuilder;
        }

        /// <inheritdoc/>
        public IAnimationBuilder End() => _parent ?? this;

        private ITransformationBuilder AddAction(IAnimationAction action)
        {
            Actions.Add(action);
            return this;
        }

        /// <inheritdoc/>
        public IEnumerator Play()
        {
            if (_isParallel)
            {
                var block = new ParallelBlock(null);
                foreach(var a in Actions) block.InternalBuilder.Actions.Add(a);
                return block.Execute();
            }
            else
            {
                return ExecuteSequential();
            }
        }

        private IEnumerator ExecuteSequential()
        {
            foreach (var action in Actions)
            {
                yield return action.Execute();
            }
        }

        // Methods for setting transformation parameters (destination, duration, easing, etc.).
        /// <inheritdoc/>
        public ITransformationBuilder To(Vector3 position) { (Actions[Actions.Count - 1] as ITweenAction)?.SetTarget(position); return this; }
        /// <inheritdoc/>
        public ITransformationBuilder To(GameObject target) { (Actions[Actions.Count - 1] as ITweenAction)?.SetTarget(target); return this; }
        /// <inheritdoc/>
        public ITransformationBuilder To(GameObject target, string anchorTag) { (Actions[Actions.Count - 1] as ITweenAction)?.SetTargetWithTag(target, anchorTag); return this; }
        /// <inheritdoc/>
        public ITransformationBuilder By(Vector3 axis) { (Actions[Actions.Count - 1] as ITweenAction)?.SetDelta(axis); return this; }
        /// <inheritdoc/>
        public ITransformationBuilder To(Quaternion rotation) { (Actions[Actions.Count - 1] as ITweenAction)?.SetTarget(rotation); return this; }
        /// <inheritdoc/>
        public ITransformationBuilder Duration(float seconds) { (Actions[Actions.Count - 1] as ITweenAction)?.SetDuration(seconds); return this; }
        /// <inheritdoc/>
        public ITransformationBuilder WithEasing(EasingType type) { (Actions[Actions.Count - 1] as ITweenAction)?.SetEasing(type); return this; }
        /// <inheritdoc/>
        public ITransformationBuilder InLocalSpace() { (Actions[Actions.Count - 1] as ITweenAction)?.SetLocal(true); return this; }
    }

    internal interface ITweenAction
    {
        void SetTarget(Vector3 pos);
        void SetTarget(GameObject target);
        void SetTargetWithTag(GameObject target, string anchorTag);
        void SetTarget(Quaternion rot);
        void SetDelta(Vector3 delta);
        void SetDuration(float duration);
        void SetEasing(EasingType type);
        void SetLocal(bool local);
    }

    // Define the specific animation action implementations for moving, rotating, scaling, and more.

    /// <summary>
    /// An action that executes immediately.
    /// </summary>
    public class InstantAction : IAnimationAction
    {
        private readonly Action _action;
        public InstantAction(Action action) => _action = action;
        public IEnumerator Execute() { _action?.Invoke(); yield break; }
    }

    /// <summary>
    /// An action that waits for a specified duration.
    /// </summary>
    public class WaitAction : IAnimationAction
    {
        private readonly float _seconds;
        public WaitAction(float seconds) => _seconds = seconds;
        public IEnumerator Execute()
        {
            float elapsed = 0;
            while (elapsed < _seconds)
            {
                if (Controller.Instance != null && Controller.Instance.IsPaused)
                {
                    yield return null;
                    continue;
                }
                float speed = Controller.Instance != null ? Controller.Instance.AnimationSpeed : 1.0f;
                elapsed += Time.deltaTime * speed;
                yield return null;
            }
        }
    }

    /// <summary>
    /// Base class for tweened animation actions.
    /// </summary>
    public abstract class TweenAction : IAnimationAction, ITweenAction
    {
        protected GameObject Target;
        protected string Tag;
        protected float TimeSeconds = 0.5f;
        protected EasingType EasingType = EasingType.Linear;
        protected bool IsLocal = false;

        protected TweenAction(GameObject target, string tag = null)
        {
            Target = target;
            Tag = tag;
        }

        public virtual void SetTarget(Vector3 pos) { }
        public virtual void SetTarget(GameObject target) { }
        public virtual void SetTargetWithTag(GameObject target, string anchorTag) { }
        public virtual void SetTarget(Quaternion rot) { }
        public virtual void SetDelta(Vector3 delta) { }
        public void SetDuration(float duration) => TimeSeconds = duration;
        public void SetEasing(EasingType type) => EasingType = type;
        public void SetLocal(bool local) => IsLocal = local;

        /// <inheritdoc/>
        public abstract IEnumerator Execute();

        protected Transform ResolveTarget()
        {
            if (Target == null) return null;
            if (string.IsNullOrEmpty(Tag)) return Target.transform;

            var metadata = Target.GetComponent<ProblemObjectMetaData>();
            if (metadata != null)
            {
                var anchor = metadata.GetAnchor(Tag);
                if (anchor != null) return anchor;
            }

            Debug.LogWarning($"[PDSim] Tag '{Tag}' not found on '{Target.name}'. Using root.");
            return Target.transform;
        }

        protected IEnumerator DoTween(Action<float> update)
        {
            if (Target == null) yield break;
            if (TimeSeconds <= 0) { update(1f); yield break; }

            float elapsed = 0;
            while (elapsed < TimeSeconds)
            {
                if (Controller.Instance != null && Controller.Instance.IsPaused)
                {
                    yield return null;
                    continue;
                }

                float speed = Controller.Instance != null ? Controller.Instance.AnimationSpeed : 1.0f;
                elapsed += Time.deltaTime * speed;
                float t = Mathf.Clamp01(elapsed / TimeSeconds);
                update(Easing.Apply(t, EasingType));
                yield return null;
            }
            update(1f);
        }
    }

    /// <summary>
    /// Action that moves a GameObject.
    /// </summary>
    public class MoveAction : TweenAction
    {
        private Vector3 _start;
        private Vector3 _end;
        private GameObject _endTarget;
        private string _endTargetTag;
        // Offset added to every resolved end position when moving the root to place an anchor.
        private Vector3 _anchorToRootOffset = Vector3.zero;

        public MoveAction(GameObject target, string tag = null) : base(target, tag) { }

        public override void SetTarget(Vector3 pos) => _end = pos;
        public override void SetTarget(GameObject target) => _endTarget = target;
        public override void SetTargetWithTag(GameObject target, string anchorTag)
        {
            _endTarget    = target;
            _endTargetTag = anchorTag;
        }

        // Resolves the world-space destination and applies the anchor-to-root offset so that
        // the root object is positioned such that the anchor reaches the requested point.
        private Vector3 ResolveEndPosition()
        {
            Vector3 basePos;
            if (_endTarget == null)
            {
                basePos = _end;
            }
            else if (!string.IsNullOrEmpty(_endTargetTag))
            {
                var meta   = _endTarget.GetComponent<ProblemObjectMetaData>();
                var anchor = meta?.GetAnchor(_endTargetTag);
                if (anchor != null)
                    basePos = anchor.localPosition;
                else
                {
                    Debug.LogWarning($"[PDSim] Destination tag '{_endTargetTag}' not found on '{_endTarget.name}'. Using root.");
                    basePos = _endTarget.transform.localPosition;
                }
            }
            else
            {
                basePos = _endTarget.transform.position;
            }
            return basePos + _anchorToRootOffset;
        }

        /// <inheritdoc/>
        public override IEnumerator Execute()
        {
            var resolvedSource = ResolveTarget();
            if (resolvedSource == null) yield break;

            // When an anchor tag is specified, Move(obj, "Anchor").To(dest) should move the
            // root object so that the anchor ends up at dest — not move the anchor child itself.
            // Compute the offset from anchor to root once, then redirect movement to the root.
            if (!string.IsNullOrEmpty(Tag) && resolvedSource != Target.transform)
            {
                _anchorToRootOffset = Target.transform.position - resolvedSource.position;
                resolvedSource = Target.transform;
            }

            // If the moving object is marked for NavMesh movement, use the agent.
            var visObj = Target.GetComponent<VisualisationObject>();
            if (visObj != null && visObj.UseNavMeshAgent)
                yield return ExecuteNavMesh(resolvedSource, visObj);
            else
                yield return ExecuteLerp(resolvedSource);
        }

        private IEnumerator ExecuteLerp(Transform resolvedSource)
        {
            _start = IsLocal ? resolvedSource.localPosition : resolvedSource.position;
            _end   = ResolveEndPosition();

            yield return DoTween(t =>
            {
                if (resolvedSource == null) return;
                var val = Vector3.Lerp(_start, _end, t);
                if (IsLocal) resolvedSource.localPosition = val;
                else         resolvedSource.position      = val;
            });
        }

        private IEnumerator ExecuteNavMesh(Transform resolvedSource, VisualisationObject visObj)
        {
            var agent = Target.GetComponent<NavMeshAgent>();
            if (agent == null)
            {
                // NavMesh agent unexpectedly missing — fall back to lerp.
                Debug.LogWarning($"[PDSim] '{Target.name}' has useNavMeshAgent=true but no NavMeshAgent component. Falling back to lerp.");
                yield return ExecuteLerp(resolvedSource);
                yield break;
            }

            // Apply movement settings (or defaults).
            var s = visObj.MovementSettings;
            float baseSpeed       = s != null ? s.Speed           : 1f;
            float angularSpeed    = s != null ? s.AngularSpeed    : 120f;
            float acceleration    = s != null ? s.Acceleration    : 8f;
            float stoppingDist    = s != null ? s.StoppingDistance : 0.1f;

            agent.angularSpeed    = angularSpeed;
            agent.acceleration    = acceleration;
            agent.stoppingDistance = stoppingDist;
            agent.isStopped       = false;
            agent.SetDestination(ResolveEndPosition());

            // Wait for the path to be computed.
            while (agent.pathPending)
                yield return null;

            // Drive toward destination, respecting pause and animation speed.
            while (agent.remainingDistance > agent.stoppingDistance)
            {
                // Update destination each frame in case the target is moving.
                if (_endTarget != null)
                    agent.SetDestination(ResolveEndPosition());

                if (Controller.Instance != null && Controller.Instance.IsPaused)
                {
                    agent.isStopped = true;
                    yield return null;
                    continue;
                }

                agent.isStopped = false;
                float speedScale = Controller.Instance != null ? Controller.Instance.AnimationSpeed : 1f;
                agent.speed = baseSpeed * speedScale;
                yield return null;
            }

            agent.isStopped = false;
            // Snap to exact destination so floating-point drift doesn't accumulate.
            resolvedSource.position = ResolveEndPosition();
        }
    }

    /// <summary>
    /// Action that rotates a GameObject.
    /// </summary>
    public class RotateAction : TweenAction
    {
        private Quaternion _start;
        private Quaternion _end;
        private Vector3 _delta;

        public RotateAction(GameObject target, string tag = null) : base(target, tag) { }
        public override void SetTarget(Quaternion rot) => _end = rot;
        public override void SetTarget(Vector3 euler) => _end = Quaternion.Euler(euler);
        public override void SetDelta(Vector3 delta) => _delta = delta;

        /// <inheritdoc/>
        public override IEnumerator Execute()
        {
            var resolvedTarget = ResolveTarget();
            if (resolvedTarget == null) yield break;

            _start = IsLocal ? resolvedTarget.localRotation : resolvedTarget.rotation;
            if (_delta != Vector3.zero) _end = _start * Quaternion.Euler(_delta);

            yield return DoTween(t => {
                if (resolvedTarget == null) return;
                var val = Quaternion.Slerp(_start, _end, t);
                if (IsLocal) resolvedTarget.localRotation = val;
                else resolvedTarget.rotation = val;
            });
        }
    }

    /// <summary>
    /// Action that scales a GameObject.
    /// </summary>
    public class ScaleAction : TweenAction
    {
        private Vector3 _start;
        private Vector3 _end;

        public ScaleAction(GameObject target, string tag = null) : base(target, tag) { }
        public override void SetTarget(Vector3 scale) => _end = scale;

        /// <inheritdoc/>
        public override IEnumerator Execute()
        {
            var resolvedTarget = ResolveTarget();
            if (resolvedTarget == null) yield break;

            _start = resolvedTarget.localScale;
            yield return DoTween(t => {
                if (resolvedTarget == null) return;
                resolvedTarget.localScale = Vector3.Lerp(_start, _end, t);
            });
        }
    }

    /// <summary>
    /// Action that changes the color of a GameObject's material.
    /// </summary>
    public class ColorAction : TweenAction
    {
        private Color _start;
        private Color _end;
        private Renderer _renderer;
        private MaterialPropertyBlock _propBlock;

        public ColorAction(GameObject target, Color end) : base(target) { _end = end; }
        public ColorAction(GameObject target, string tag, Color end) : base(target, tag) { _end = end; }

        /// <inheritdoc/>
        public override IEnumerator Execute()
        {
            if (Target == null) yield break;

            if (!string.IsNullOrEmpty(Tag))
            {
                var metadata = Target.GetComponent<ProblemObjectMetaData>();
                _renderer = metadata?.GetRender(Tag);
            }

            if (_renderer == null) _renderer = Target.GetComponent<Renderer>();
            if (_renderer == null) { yield break; }

            _propBlock = new MaterialPropertyBlock();
            _renderer.GetPropertyBlock(_propBlock);
            _start = _renderer.material.color;

            yield return DoTween(t => {
                if (_renderer == null) return;
                var c = Color.Lerp(_start, _end, t);
                _propBlock.SetColor("_Color", c);
                _propBlock.SetColor("_BaseColor", c);
                _renderer.SetPropertyBlock(_propBlock);
            });
        }
    }

    /// <summary>
    /// A block of actions that execute in parallel.
    /// </summary>
    public class ParallelBlock : IAnimationAction
    {
        internal readonly AnimationBuilder InternalBuilder;
        public ParallelBlock(AnimationBuilder parent) { InternalBuilder = new AnimationBuilder(true, parent); }

        /// <inheritdoc/>
        public IEnumerator Execute()
        {
            var enumerators = new List<IEnumerator>();
            foreach (var action in InternalBuilder.Actions)
            {
                enumerators.Add(action.Execute());
            }

            while (enumerators.Count > 0)
            {
                for (int i = enumerators.Count - 1; i >= 0; i--)
                {
                    if (!enumerators[i].MoveNext())
                    {
                        enumerators.RemoveAt(i);
                    }
                }
                yield return null;
            }
        }
    }

    /// <summary>
    /// Action that updates text on a GameObject.
    /// </summary>
    public class TextAction : IAnimationAction
    {
        private readonly GameObject _target;
        private readonly string _tag;
        private readonly string _text;

        public TextAction(GameObject target, string tag, string text)
        {
            _target = target;
            _tag = tag;
            _text = text;
        }

        /// <inheritdoc/>
        public IEnumerator Execute()
        {
            if (_target == null) yield break;
            var metadata = _target.GetComponent<ProblemObjectMetaData>();
            var display = metadata?.GetUI(_tag);

            if (display == null)
            {
                Debug.LogWarning($"[PDSim] UI tag '{_tag}' not found on '{_target.name}'.");
                yield break;
            }

            var tmp = display.GetComponent("TMPro.TMP_Text"); //default supported
            if (tmp != null)
            {
                tmp.GetType().GetProperty("text")?.SetValue(tmp, _text);
            }
            else if (display is Text textUI) //legacy for quick prototype
            {
                textUI.text = _text;
            }
            else if (display is UIDocument uiDoc)
            {
                var element = uiDoc.rootVisualElement.Q<TextElement>(_tag);
                if (element != null) element.text = _text;
            }

            yield break;
        }
    }
}
