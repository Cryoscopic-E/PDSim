using GeTPlan.Core.Models; using GeTPlan.Core.Logic; using GeTPlan.Core.Models.Expressions; using PDSimAPI;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace PDSim.SceneUI
{
    public class PlanListUI : MonoBehaviour
    {
        private ListView _planList;
        private PlanActionsListController _actionListController;
        private MovablePanel _movablePanel;
        private VisualElement _root;
        private bool _visible;

        private void OnEnable()
        {
            var uiDocument = GetComponent<UIDocument>();
            var docRoot = uiDocument.rootVisualElement;
            docRoot.styleSheets.Add(Resources.Load<StyleSheet>("SceneUI/SceneUSS"));

            // Root panel
            _root = new VisualElement { name = "Root" };
            _root.AddToClassList("panel");
            _root.AddToClassList("panel--plan");
            _root.style.display = DisplayStyle.None;
            docRoot.Add(_root);

            // Title
            var title = new Label("Plan Actions") { enableRichText = true };
            title.AddToClassList("panel-title");
            _root.Add(title);

            // List container
            var listContainer = new VisualElement { name = "ListContainer" };
            listContainer.AddToClassList("panel-list-container");
            _root.Add(listContainer);

            // ListView
            _planList = new ListView
            {
                name = "PlanList",
                viewDataKey = "PlanList",
                fixedItemHeight = 40,
                pickingMode = PickingMode.Ignore,
                selectionType = SelectionType.None,
                focusable = false
            };
            _planList.AddToClassList("panel-listview");
            listContainer.Add(_planList);

            _movablePanel = new MovablePanel(_root);
        }

        public PlanActionsListController InitializePlanList(List<GroundedAction> list)
        {
            _actionListController = new PlanActionsListController();
            _actionListController.SetPlanActions(list);
            _actionListController.InitializeActionList(_root);
            return _actionListController;
        }

        public void ToggleVisibility()
        {
            _movablePanel.ResetPosition();
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

        public void HighlightCurrentAction(int index)
        {
            _planList.AddToSelection(index);
        }
    }
}
