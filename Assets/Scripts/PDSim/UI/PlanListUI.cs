using System.Collections.Generic;
using PDSim.Components;
using UnityEngine;
using UnityEngine.UIElements;

namespace PDSim.UI
{
    public class PlanListUI : MonoBehaviour
    {
        [SerializeField]
        VisualTreeAsset actionItemTemplate;

        private ListView _planList;
        private PlanActionsListController _actionListController;
        private MovablePanel _movablePanel;
        private VisualElement _root;

        private void OnEnable()
        {
            _root = GetComponent<UIDocument>().rootVisualElement;

            _movablePanel = new(_root);

            _planList = _root.Q<ListView>("PlanList");
        }

        public PlanActionsListController InitializePlanList(List<PdPlanAction> list)
        {
            _actionListController = new PlanActionsListController();
            _actionListController.SetPlanActions(list);
            _actionListController.InitializeActionList(_root, actionItemTemplate);
            return _actionListController;
        }

        public void ToggleVisibility()
        {
            _root.style.display = _root.style.display == DisplayStyle.None ? DisplayStyle.Flex : DisplayStyle.None;
            _movablePanel.ResetPosition();
        }

        public void HighlightCurrentAction(int index)
        {
            _planList.AddToSelection(index);
        }
    }
}
