using UnityEngine;
using UnityEngine.UIElements;

namespace PDSim.SceneUI
{
    /// <summary>
    /// Manipulator that allows a visual element to be dragged within its panel.
    /// </summary>
    public class MovablePanel : PointerManipulator
    {
        #region Private Fields
        private Vector2 _targetStartPosition;
        private Vector3 _pointerStartPosition;
        private bool _isEnabled;
        #endregion

        #region Constructor
        /// <summary>
        /// Initializes a new instance of the MovablePanel manipulator.
        /// </summary>
        /// <param name="target">The visual element to make movable.</param>
        public MovablePanel(VisualElement target)
        {
            this.target = target;
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Resets the target's translation to (0, 0).
        /// </summary>
        public void ResetPosition()
        {
            var translate = new Translate(0, 0);
            target.style.translate = new StyleTranslate(translate);
        }
        #endregion

        #region Protected Methods
        protected override void RegisterCallbacksOnTarget()
        {
            target.RegisterCallback<PointerDownEvent>(OnMouseDown);
            target.RegisterCallback<PointerMoveEvent>(OnMouseMove);
            target.RegisterCallback<PointerUpEvent>(OnMouseUp);
        }

        protected override void UnregisterCallbacksFromTarget()
        {
            target.UnregisterCallback<PointerDownEvent>(OnMouseDown);
            target.UnregisterCallback<PointerMoveEvent>(OnMouseMove);
            target.UnregisterCallback<PointerUpEvent>(OnMouseUp);
        }
        #endregion

        #region Private Event Handlers
        private void OnMouseDown(PointerDownEvent evt)
        {
            _targetStartPosition = target.resolvedStyle.translate;
            _pointerStartPosition = evt.position;
            target.CapturePointer(evt.pointerId);
            _isEnabled = true;
        }

        private void OnMouseMove(PointerMoveEvent evt)
        {
            if (_isEnabled && target.HasPointerCapture(evt.pointerId))
            {
                Vector3 pointerDelta = evt.position - _pointerStartPosition;

                var newPosition = new Vector2(
                    Mathf.Clamp(_targetStartPosition.x + pointerDelta.x, 0, target.panel.visualTree.worldBound.width),
                    Mathf.Clamp(_targetStartPosition.y + pointerDelta.y, 0, target.panel.visualTree.worldBound.height));

                target.style.translate = new StyleTranslate(new Translate(newPosition.x, newPosition.y));
            }
        }

        private void OnMouseUp(PointerUpEvent evt)
        {
            if (_isEnabled && target.HasPointerCapture(evt.pointerId))
            {
                target.ReleasePointer(evt.pointerId);
                _isEnabled = false;
            }
        }
        #endregion
    }
}
