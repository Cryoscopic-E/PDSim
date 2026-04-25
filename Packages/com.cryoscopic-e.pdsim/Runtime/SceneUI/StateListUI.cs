using PDSim.Components;
using UnityEngine;
using UnityEngine.UIElements;

namespace PDSim.SceneUI
{
    /// <summary>
    /// Manages the State List UI panel, showing fluents and values for a selected object.
    /// </summary>
    public class StateListUI : MonoBehaviour
    {
        #region Private Fields
        private Label _objectName;
        private ListView _stateList;
        private StateListController _stateListController;
        private VisualElement _root;
        private bool _visible;
        #endregion

        #region Unity Lifecycle
        private void OnEnable()
        {
            var uiDocument = GetComponent<UIDocument>();
            var docRoot = uiDocument.rootVisualElement;
            docRoot.styleSheets.Add(Resources.Load<StyleSheet>("SceneUI/SceneUSS"));

            // Root panel: responsive positioning via USS (bottom-right anchor)
            _root = new VisualElement { name = "Root" };
            _root.AddToClassList("panel");
            _root.AddToClassList("panel--state");
            _root.style.display = DisplayStyle.None;
            docRoot.Add(_root);

            // Object name label
            _objectName = new Label { name = "ObjectName" };
            _objectName.AddToClassList("panel-title");
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
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Initializes the state list for a specific simulation object.
        /// </summary>
        /// <param name="simObject">The simulation object whose state to display.</param>
        /// <returns>The initialized StateListController.</returns>
        public StateListController InitializeList(VisualisationObject simObject)
        {
            _objectName.text = simObject.name;
            _stateListController.InitializeStateList(_root);
            _stateListController.SetState(simObject.GetObjectState());
            return _stateListController;
        }

        /// <summary>
        /// Clears the state list display.
        /// </summary>
        public void Clear()
        {
            _objectName.text = "";
            _stateListController.ClearList();
        }

        /// <summary>
        /// Toggles the visibility of the State List panel.
        /// </summary>
        public void ToggleVisibility()
        {
            _visible = !_visible;
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
