using PDSim.Components;
using UnityEngine;
using UnityEngine.UIElements;

namespace PDSim.SceneUI
{
    public class StateListUI : MonoBehaviour
    {
        private Label objectName;
        private ListView _stateList;
        private StateListController _stateListController;
        private VisualElement _root;
        private bool _visible;

        private void OnEnable()
        {
            var uiDocument = GetComponent<UIDocument>();
            var docRoot = uiDocument.rootVisualElement;
            docRoot.styleSheets.Add(Resources.Load<StyleSheet>("SceneUI/SceneUSS"));

            // Root panel — responsive positioning via USS (bottom-right anchor)
            _root = new VisualElement { name = "Root" };
            _root.AddToClassList("panel");
            _root.AddToClassList("panel--state");
            _root.style.display = DisplayStyle.None;
            docRoot.Add(_root);

            // Object name label
            objectName = new Label { name = "ObjectName" };
            objectName.AddToClassList("panel-title");
            _root.Add(objectName);

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

        public StateListController InitializeList(VisualisationObject simObject)
        {
            objectName.text = simObject.name;
            _stateListController.InitializeStateList(_root);
            _stateListController.SetState(simObject.GetObjectState());
            return _stateListController;
        }

        public void Clear()
        {
            objectName.text = "";
            _stateListController.ClearList();
        }

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
    }
}
