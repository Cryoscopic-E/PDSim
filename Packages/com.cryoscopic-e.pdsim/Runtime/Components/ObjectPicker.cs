using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

namespace PDSim.Components
{
    /// <summary>
    /// Central mouse picker for visualisation objects, driven by the Input System package.
    /// Raycasts from the main camera every frame to detect hover, and handles
    /// click-to-select / click-away-to-deselect. Replaces the legacy OnMouseEnter/OnMouseExit
    /// callbacks, which never fire when the project uses the Input System package only.
    /// </summary>
    public class ObjectPicker : MonoBehaviour
    {
        #region Serialized Fields

        [Tooltip("Maximum raycast distance for picking objects.")]
        [SerializeField] private float MaxPickDistance = 500f;

        [Tooltip("Layers considered when picking objects.")]
        [SerializeField] private LayerMask PickMask = ~0;

        #endregion

        #region Unity Lifecycle

        private void Start()
        {
            _camera = Camera.main;
            _uiDocuments = FindObjectsByType<UIDocument>(FindObjectsSortMode.None);
        }

        private void Update()
        {
            var mouse = Mouse.current;
            if (mouse == null)
                return;

            if (_camera == null)
            {
                _camera = Camera.main;
                if (_camera == null)
                    return;
            }

            // First-person camera mode locks the cursor — suspend picking.
            if (UnityEngine.Cursor.lockState != CursorLockMode.None)
            {
                SetHovered(null);
                return;
            }

            var screenPos = mouse.position.ReadValue();

            if (IsPointerOverUI(screenPos))
            {
                SetHovered(null);
                return;
            }

            VisualisationObject hit = null;
            var ray = _camera.ScreenPointToRay(screenPos);
            if (Physics.Raycast(ray, out var hitInfo, MaxPickDistance, PickMask))
                hit = hitInfo.collider.GetComponentInParent<VisualisationObject>();

            SetHovered(hit);

            if (mouse.leftButton.wasPressedThisFrame)
                SetSelected(hit);

            var keyboard = Keyboard.current;
            if (keyboard != null && keyboard.escapeKey.wasPressedThisFrame)
                SetSelected(null);
        }

        #endregion

        #region Private Internals

        private Camera _camera;
        private UIDocument[] _uiDocuments;
        private VisualisationObject _hovered;
        private VisualisationObject _selected;

        private void SetHovered(VisualisationObject obj)
        {
            if (obj == _hovered)
                return;

            if (_hovered != null && _hovered != _selected)
                _hovered.SetHighlight(HighlightState.None);

            _hovered = obj;

            if (_hovered != null)
            {
                if (_hovered != _selected)
                    _hovered.SetHighlight(HighlightState.Hovered);
                ProblemObjects.Instance.HoverObject(_hovered);
            }
            else
            {
                ProblemObjects.Instance.ClearHover();
            }
        }

        private void SetSelected(VisualisationObject obj)
        {
            if (obj == _selected)
                return;

            if (_selected != null)
                _selected.SetHighlight(_selected == _hovered ? HighlightState.Hovered : HighlightState.None);

            _selected = obj;

            if (_selected != null)
            {
                _selected.SetHighlight(HighlightState.Selected);
                ProblemObjects.Instance.SelectObject(_selected);
            }
            else
            {
                ProblemObjects.Instance.ClearSelection();
            }
        }

        /// <summary>
        /// Returns true when the pointer is over a pickable UI Toolkit element.
        /// Full-screen layout containers must use PickingMode.Ignore or they
        /// would swallow every position on screen.
        /// </summary>
        private bool IsPointerOverUI(Vector2 screenPos)
        {
            // UI Toolkit panels use a top-left origin, screen space is bottom-left.
            var panelSpace = new Vector2(screenPos.x, Screen.height - screenPos.y);

            foreach (var document in _uiDocuments)
            {
                if (document == null || document.rootVisualElement?.panel == null)
                    continue;

                var panel = document.rootVisualElement.panel;
                var panelPos = RuntimePanelUtils.ScreenToPanel(panel, panelSpace);
                if (panel.Pick(panelPos) != null)
                    return true;
            }

            return false;
        }

        #endregion
    }
}
