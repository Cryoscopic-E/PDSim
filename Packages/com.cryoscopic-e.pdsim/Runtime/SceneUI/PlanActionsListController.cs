using GeTPlan.Core.Models;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace PDSim.SceneUI
{
    /// <summary>
    /// Controller for the Plan Actions ListView.
    /// Manages the binding of action data to visual elements in the list.
    /// </summary>
    public class PlanActionsListController
    {
        #region Private Fields
        private ListView _actionsList;
        private List<GroundedAction> _planActions;
        #endregion

        #region Initialization
        /// <summary>
        /// Initializes the action list by querying the ListView from the root element.
        /// </summary>
        /// <param name="root">The root visual element containing the ListView.</param>
        public void InitializeActionList(VisualElement root)
        {
            _actionsList = root.Q<ListView>("PlanList");
            FillActionsList();
        }

        /// <summary>
        /// Sets the plan actions data source.
        /// </summary>
        /// <param name="actions">List of grounded actions.</param>
        public void SetPlanActions(List<GroundedAction> actions)
        {
            _planActions = actions;
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Configures the ListView callbacks for making and binding items.
        /// </summary>
        private void FillActionsList()
        {
            _actionsList.makeItem = () =>
            {
                var label = new Label { name = "Item" };
                label.AddToClassList("list-item");

                var actionEntry = new ActionEntryController();
                label.userData = actionEntry;
                actionEntry.SetVisualElement(label);

                return label;
            };

            _actionsList.bindItem = (item, index) =>
            {
                (item.userData as ActionEntryController).SetActionData(_planActions[index]);
            };

            _actionsList.fixedItemHeight = 45;
            _actionsList.itemsSource = _planActions;
        }
        #endregion
    }
}
