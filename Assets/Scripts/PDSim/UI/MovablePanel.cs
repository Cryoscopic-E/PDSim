using UnityEngine;
using UnityEngine.UIElements;

namespace PDSim.UI
{
    public class MovablePanel : PointerManipulator
    {

        public MovablePanel(VisualElement target)
        {
            this.target = target;
        }

        public void ResetPosition()
        {
            target.transform.position = Vector2.zero;
        }
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

        private Vector2 targetStartPosition { get; set; }

        private Vector3 pointerStartPosition { get; set; }

        private bool enabled { get; set; }


        private void OnMouseDown(PointerDownEvent evt)
        {
            targetStartPosition = target.transform.position;
            pointerStartPosition = evt.position;
            target.CapturePointer(evt.pointerId);
            enabled = true;
        }

        private void OnMouseMove(PointerMoveEvent evt)
        {
            if (enabled && target.HasPointerCapture(evt.pointerId))
            {
                Vector3 pointerDelta = evt.position - pointerStartPosition;

                target.transform.position = new Vector2(
                    Mathf.Clamp(targetStartPosition.x + pointerDelta.x, 0, target.panel.visualTree.worldBound.width),
                    Mathf.Clamp(targetStartPosition.y + pointerDelta.y, 0, target.panel.visualTree.worldBound.height));
            }
        }

        private void OnMouseUp(PointerUpEvent evt)
        {
            if (enabled && target.HasPointerCapture(evt.pointerId))
            {
                target.ReleasePointer(evt.pointerId);
            }
        }
    }
}
