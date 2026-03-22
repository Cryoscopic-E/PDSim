using GeTPlan.Core.Models; using GeTPlan.Core.Logic; using GeTPlan.Core.Models.Expressions; using PDSimAPI;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace PDSim.SceneUI
{
    public class PlanActionsListController
    {
        ListView actionsList;
        private List<GroundedAction> planActions;

        public void InitializeActionList(VisualElement root)
        {
            actionsList = root.Q<ListView>("PlanList");
            FillActionsList();
        }

        public void SetPlanActions(List<GroundedAction> actions)
        {
            planActions = actions;
        }

        void FillActionsList()
        {
            actionsList.makeItem = () =>
            {
                var label = new Label { name = "Item" };
                label.AddToClassList("list-item");

                var actionEntry = new ActionEntryController();
                label.userData = actionEntry;
                actionEntry.SetVisualElement(label);

                return label;
            };

            actionsList.bindItem = (item, index) =>
            {
                (item.userData as ActionEntryController).SetActionData(planActions[index]);
            };

            actionsList.fixedItemHeight = 45;
            actionsList.itemsSource = planActions;
        }
    }
}
