using PDSim.Components;
using UnityEngine;
using UnityEngine.UIElements;

namespace PDSim.SceneUI
{
    /// <summary>
    /// Manages the State List UI panel, showing fluents and values for the selected object.
    /// The panel is pinned open when an object is selected (clicked) and refreshes live
    /// while the pinned object's state changes during plan replay.
    /// </summary>
    public class StateListUI : MonoBehaviour
    {
        #region Private Fields
        private Label _objectName;
        private ListView _stateList;
        private StateListController _stateListController;
        private VisualElement _root;
        private bool _visible;
        private VisualisationObject _pinnedObject;
        #endregion

        #region Unity Lifecycle
        private void OnEnable()
        {
            var uiDocument = GetComponent<UIDocument>();
            var docRoot = uiDocument.rootVisualElement;
            docRoot.styleSheets.Add(Resources.Load<StyleSheet>("SceneUI/SceneUSS"));

            // The document root spans the whole screen — it must not swallow
            // scene picking done by the ObjectPicker.
            docRoot.pickingMode = PickingMode.Ignore;

            // Root panel: responsive positioning via USS (bottom-right anchor)
            _root = new VisualElement { name = "Root" };
            _root.AddToClassList("panel");
            _root.AddToClassList("panel--state");
            _root.style.display = DisplayStyle.None;
            docRoot.Add(_root);

            // Object name label
            _objectName = new Label { name = "ObjectName" };
            _objectName.AddToClassList("panel-title");
            _objectName.text = EmptyTitle;
            _root.Add(_objectName);

            // List container
            var listContainer = new VisualElement { name = "ListContainer" };
            listContainer.AddToClassList("panel-list-container");
            _root.Add(listContainer);

            // ListView
            _stateList = new ListView
            {
                name = "StateList",
                viewDataKey = "StateList",
                fixedItemHeight = 40,
                pickingMode = PickingMode.Ignore,
                selectionType = SelectionType.None,
                focusable = false
            };
            _stateList.AddToClassList("panel-listview");
            listContainer.Add(_stateList);

            _stateListController = new StateListController();
            _stateListController.InitializeStateList(_root);
        }

        private void OnDisable()
        {
            Unpin();
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Pins the panel to a simulation object: shows its state and keeps it
        /// updated until another object is selected or the selection is cleared.
        /// </summary>
        /// <param name="simObject">The simulation object whose state to display.</param>
        public void ShowFor(VisualisationObject simObject)
        {
            Unpin();
            _pinnedObject = simObject;
            _pinnedObject.OnStateChanged += RefreshState;

            _objectName.text = $"{simObject.name} ({simObject.ObjectType})";
            _stateListController.SetState(simObject.GetObjectState());
            SetVisible(true);
        }

        /// <summary>
        /// Unpins the current object, clears the list, and hides the panel.
        /// </summary>
        public void Hide()
        {
            Unpin();
            _objectName.text = EmptyTitle;
            _stateListController.ClearList();
            SetVisible(false);
        }

        /// <summary>
        /// Toggles the visibility of the State List panel (manual override button).
        /// </summary>
        public void ToggleVisibility()
        {
            SetVisible(!_visible);
        }
        #endregion

        #region Private Methods
        private const string EmptyTitle = "No object selected";

        private void RefreshState()
        {
            if (_pinnedObject != null)
                _stateListController.SetState(_pinnedObject.GetObjectState());
        }

        private void Unpin()
        {
            if (_pinnedObject == null) return;
            _pinnedObject.OnStateChanged -= RefreshState;
            _pinnedObject = null;
        }

        private void SetVisible(bool value)
        {
            if (_visible == value) return;
            _visible = value;
            if (_visible)
            {
                _root.style.display = DisplayStyle.Flex;
                _root.schedule.Execute(() => _root.AddToClassList("panel--visible"));
            }
            else
            {
                _root.RemoveFromClassList("panel--visible");
                _root.schedule.Execute(() => _root.style.display = DisplayStyle.None).StartingIn(200);
            }
        }
        #endregion
    }
}
