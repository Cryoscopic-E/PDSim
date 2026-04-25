using GeTPlan.Core.Models.Expressions;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace PDSim.SceneUI
{
    /// <summary>
    /// Controller for the State List ListView.
    /// Manages the binding of object state (fluents) to visual elements.
    /// </summary>
    public class StateListController
    {
        #region Private Fields
        private ListView _stateList;
        private List<(FluentExpression Fluent, object Value)> _state = new();
        #endregion

        #region Initialization
        /// <summary>
        /// Initializes the state list by querying the ListView from the root element.
        /// </summary>
        /// <param name="root">The root visual element containing the ListView.</param>
        public void InitializeStateList(VisualElement root)
        {
            _stateList = root.Q<ListView>("StateList");
            SetupList();
        }

        /// <summary>
        /// Sets the state data source and rebuilds the list.
        /// </summary>
        /// <param name="fluents">List of fluents and their values.</param>
        public void SetState(List<(FluentExpression Fluent, object Value)> fluents)
        {
            _state = fluents;
            _stateList.itemsSource = _state;
            _stateList.Rebuild();
        }

        /// <summary>
        /// Clears the state list.
        /// </summary>
        public void ClearList()
        {
            _state.Clear();
            _stateList.itemsSource?.Clear();
            _stateList.Rebuild();
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Configures the ListView callbacks for making and binding items.
        /// </summary>
        private void SetupList()
        {
            _stateList.makeItem = () =>
            {
                var label = new Label { name = "Item" };
                label.AddToClassList("list-item");

                var entry = new StateEntryController();
                label.userData = entry;
                entry.SetVisualElement(label);

                return label;
            };

            _stateList.bindItem = (item, index) =>
            {
                (item.userData as StateEntryController).SetData(_state[index]);
            };

            _stateList.fixedItemHeight = 45;
        }
        #endregion
    }
}
