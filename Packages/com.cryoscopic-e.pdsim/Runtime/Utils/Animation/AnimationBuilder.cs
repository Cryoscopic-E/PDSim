using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using PDSim.Components;

namespace PDSim.Utils.Animation
{
    public interface IAnimationAction
    {
        IEnumerator Execute();
    }

    public interface IAnimationBuilder
    {
        ITransformationBuilder Move(GameObject target);
        ITransformationBuilder Move(GameObject target, string anchorTag);
        ITransformationBuilder Rotate(GameObject target);
        ITransformationBuilder Rotate(GameObject target, string anchorTag);
        ITransformationBuilder Scale(GameObject target);
        IAnimationBuilder Show(GameObject target);
        IAnimationBuilder Hide(GameObject target);
        IAnimationBuilder Color(GameObject target, Color color);
        IAnimationBuilder Color(GameObject target, string aestheticTag, Color color);
        IAnimationBuilder Text(GameObject target, string displayTag, string text);
        IAnimationBuilder Attach(GameObject child, GameObject parent);
        IAnimationBuilder Detach(GameObject child);
        IAnimationBuilder Wait(float seconds);
        IAnimationBuilder Then();
        IAnimationBuilder Parallel();
        IAnimationBuilder End();
        IEnumerator Play();
    }

    public interface ITransformationBuilder : IAnimationBuilder
    {
        ITransformationBuilder To(Vector3 position);
        ITransformationBuilder To(GameObject target);
        ITransformationBuilder By(Vector3 axis);
        ITransformationBuilder To(Quaternion rotation);
        ITransformationBuilder Duration(float seconds);
        ITransformationBuilder WithEasing(EasingType type);
        ITransformationBuilder InLocalSpace();
    }

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

        public ITransformationBuilder Move(GameObject target) => AddAction(new MoveAction(target));
        public ITransformationBuilder Move(GameObject target, string anchorTag) => AddAction(new MoveAction(target, anchorTag));
        public ITransformationBuilder Rotate(GameObject target) => AddAction(new RotateAction(target));
        public ITransformationBuilder Rotate(GameObject target, string anchorTag) => AddAction(new RotateAction(target, anchorTag));
        public ITransformationBuilder Scale(GameObject target) => AddAction(new ScaleAction(target));
        public IAnimationBuilder Show(GameObject target) => AddAction(new InstantAction(() => target?.SetActive(true)));
        public IAnimationBuilder Hide(GameObject target) => AddAction(new InstantAction(() => target?.SetActive(false)));
        public IAnimationBuilder Color(GameObject target, Color color) => AddAction(new ColorAction(target, color));
        public IAnimationBuilder Color(GameObject target, string aestheticTag, Color color) => AddAction(new ColorAction(target, aestheticTag, color));
        public IAnimationBuilder Text(GameObject target, string displayTag, string text) => AddAction(new TextAction(target, displayTag, text));
        public IAnimationBuilder Attach(GameObject child, GameObject parent) => AddAction(new InstantAction(() => child?.transform.SetParent(parent?.transform, true)));
        public IAnimationBuilder Detach(GameObject child) => AddAction(new InstantAction(() => child?.transform.SetParent(null, true)));
        public IAnimationBuilder Wait(float seconds) => AddAction(new WaitAction(seconds));

        public IAnimationBuilder Then() => this;

        public IAnimationBuilder Parallel()
        {
            var parallel = new ParallelBlock(this);
            Actions.Add(parallel);
            return parallel.InternalBuilder;
        }

        public IAnimationBuilder End() => _parent ?? this;

        private ITransformationBuilder AddAction(IAnimationAction action)
        {
            Actions.Add(action);
            return this;
        }

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

        // --- Transformation Methods ---
        public ITransformationBuilder To(Vector3 position) { (Actions[Actions.Count - 1] as ITweenAction)?.SetTarget(position); return this; }
        public ITransformationBuilder To(GameObject target) { (Actions[Actions.Count - 1] as ITweenAction)?.SetTarget(target); return this; }
        public ITransformationBuilder By(Vector3 axis) { (Actions[Actions.Count - 1] as ITweenAction)?.SetDelta(axis); return this; }
        public ITransformationBuilder To(Quaternion rotation) { (Actions[Actions.Count - 1] as ITweenAction)?.SetTarget(rotation); return this; }
        public ITransformationBuilder Duration(float seconds) { (Actions[Actions.Count - 1] as ITweenAction)?.SetDuration(seconds); return this; }
        public ITransformationBuilder WithEasing(EasingType type) { (Actions[Actions.Count - 1] as ITweenAction)?.SetEasing(type); return this; }
        public ITransformationBuilder InLocalSpace() { (Actions[Actions.Count - 1] as ITweenAction)?.SetLocal(true); return this; }
    }

    internal interface ITweenAction
    {
        void SetTarget(Vector3 pos);
        void SetTarget(GameObject target);
        void SetTarget(Quaternion rot);
        void SetDelta(Vector3 delta);
        void SetDuration(float duration);
        void SetEasing(EasingType type);
        void SetLocal(bool local);
    }

    // --- Action Implementations ---

    public class InstantAction : IAnimationAction
    {
        private readonly Action _action;
        public InstantAction(Action action) => _action = action;
        public IEnumerator Execute() { _action?.Invoke(); yield break; }
    }

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
                float speed = Controller.Instance != null ? Controller.Instance.animationSpeed : 1.0f;
                elapsed += Time.deltaTime * speed;
                yield return null;
            }
        }
    }

    public abstract class TweenAction : IAnimationAction, ITweenAction
    {
        protected GameObject Target;
        protected string Tag;
        protected float TimeSeconds = 0.5f;
        protected EasingType EasingType = EasingType.Linear;
        protected bool IsLocal = false;

        public TweenAction(GameObject target, string tag = null)
        {
            Target = target;
            Tag = tag;
        }

        public virtual void SetTarget(Vector3 pos) { }
        public virtual void SetTarget(GameObject target) { }
        public virtual void SetTarget(Quaternion rot) { }
        public virtual void SetDelta(Vector3 delta) { }
        public void SetDuration(float duration) => TimeSeconds = duration;
        public void SetEasing(EasingType type) => EasingType = type;
        public void SetLocal(bool local) => IsLocal = local;

        public abstract IEnumerator Execute();

        protected Transform ResolveTarget()
        {
            if (Target == null) return null;
            if (string.IsNullOrEmpty(Tag)) return Target.transform;

            var metadata = Target.GetComponent<PDSimMetadata>();
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

                float speed = Controller.Instance != null ? Controller.Instance.animationSpeed : 1.0f;
                elapsed += Time.deltaTime * speed;
                float t = Mathf.Clamp01(elapsed / TimeSeconds);
                update(Easing.Apply(t, EasingType));
                yield return null;
            }
            update(1f);
        }
    }

    public class MoveAction : TweenAction
    {
        private Vector3 _start;
        private Vector3 _end;
        private GameObject _endTarget;

        public MoveAction(GameObject target, string tag = null) : base(target, tag) { }
        public override void SetTarget(Vector3 pos) => _end = pos;
        public override void SetTarget(GameObject target) => _endTarget = target;

        public override IEnumerator Execute()
        {
            var resolvedTarget = ResolveTarget();
            if (resolvedTarget == null) yield break;

            _start = IsLocal ? resolvedTarget.localPosition : resolvedTarget.position;
            
            yield return DoTween(t => {
                if (resolvedTarget == null) return;
                if (_endTarget != null) _end = _endTarget.transform.position;
                var val = Vector3.Lerp(_start, _end, t);
                if (IsLocal) resolvedTarget.localPosition = val;
                else resolvedTarget.position = val;
            });
        }
    }

    public class RotateAction : TweenAction
    {
        private Quaternion _start;
        private Quaternion _end;
        private Vector3 _delta;

        public RotateAction(GameObject target, string tag = null) : base(target, tag) { }
        public override void SetTarget(Quaternion rot) => _end = rot;
        public override void SetTarget(Vector3 euler) => _end = Quaternion.Euler(euler);
        public override void SetDelta(Vector3 delta) => _delta = delta;

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

    public class ScaleAction : TweenAction
    {
        private Vector3 _start;
        private Vector3 _end;

        public ScaleAction(GameObject target, string tag = null) : base(target, tag) { }
        public override void SetTarget(Vector3 scale) => _end = scale;

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

    public class ColorAction : TweenAction
    {
        private Color _start;
        private Color _end;
        private Renderer _renderer;
        private MaterialPropertyBlock _propBlock;

        public ColorAction(GameObject target, Color end) : base(target) { _end = end; }
        public ColorAction(GameObject target, string tag, Color end) : base(target, tag) { _end = end; }

        public override IEnumerator Execute()
        {
            if (Target == null) yield break;
            
            if (!string.IsNullOrEmpty(Tag))
            {
                var metadata = Target.GetComponent<PDSimMetadata>();
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

    public class ParallelBlock : IAnimationAction
    {
        internal readonly AnimationBuilder InternalBuilder;
        public ParallelBlock(AnimationBuilder parent) { InternalBuilder = new AnimationBuilder(true, parent); }

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

        public IEnumerator Execute()
        {
            if (_target == null) yield break;
            var metadata = _target.GetComponent<PDSimMetadata>();
            var display = metadata?.GetUI(_tag);

            if (display == null)
            {
                Debug.LogWarning($"[PDSim] UI tag '{_tag}' not found on '{_target.name}'.");
                yield break;
            }

            // Support TMP_Text via reflection to avoid hard dependency in base package if possible, 
            // but since we have it in asmdef, we can use it.
            // Actually, let's use GetComponent by string to be safe.
            var tmp = display.GetComponent("TMPro.TMP_Text");
            if (tmp != null)
            {
                tmp.GetType().GetProperty("text")?.SetValue(tmp, _text);
            }
            // Support UI Toolkit Label/Button/etc via UIDocument
            else if (display is UnityEngine.UIElements.UIDocument uiDoc)
            {
                var element = uiDoc.rootVisualElement.Q<UnityEngine.UIElements.TextElement>(_tag);
                if (element != null) element.text = _text;
            }

            yield break;
        }
    }
}
