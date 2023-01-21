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

        private void OnEnable()
        {
            var root = GetComponent<UIDocument>().rootVisualElement;
            _movablePanel = new(root);

            _planList = root.Q<ListView>("PlanList");
        
            var list = new List<PdAction>() {};
        
            // Initialize the character list controller
            _actionListController = new PlanActionsListController();
            _actionListController.SetPlanActions(list);
            _actionListController.InitializeActionList(root, actionItemTemplate);

            // USE TO HIGHLIGHT ACTION
            //HighlightCurrentAction(0);
        }

        public void ToggleVisibility()
        {
            gameObject.SetActive(!gameObject.activeSelf);
            _movablePanel.ResetPosition();
        }

        public void HighlightCurrentAction(int index)
        {
            _planList.AddToSelection(index);
        }
    }
}
