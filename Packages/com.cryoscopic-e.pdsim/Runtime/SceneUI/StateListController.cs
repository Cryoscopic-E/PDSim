using GeTPlan.Core.Models; using GeTPlan.Core.Logic; using GeTPlan.Core.Models.Expressions; using PDSimAPI;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace PDSim.SceneUI
{
    public class StateListController
    {
        VisualTreeAsset itemTemplate;

        ListView stateList;


        public void InitializeStateList(VisualElement root, VisualTreeAsset labelTemplate)
        {
            itemTemplate = labelTemplate;

            stateList = root.Q<ListView>("StateList");

            SetupList();
        }

        public void SetState(List<(FluentExpression Fluent, object Value)> fluents)
        {
            _state = fluents;
            stateList.itemsSource = _state;
            stateList.Rebuild();
        }

        public void ClearList()
        {
            _state.Clear();
            stateList.itemsSource.Clear();
            stateList.Rebuild();
        }

        private List<(FluentExpression Fluent, object Value)> _state = new();

        void SetupList()
        {
            // Set up a make item function for a list entry
            stateList.makeItem = () =>
            {
                // Instantiate the UXML template for the entry
                var newListEntry = itemTemplate.Instantiate();

                // Instantiate a controller for the data
                var entry = new StateEntryController();

                // Assign the controller script to the visual element
                newListEntry.userData = entry;

                // Initialize the controller script
                entry.SetVisualElement(newListEntry);

                // Return the root of the instantiated visual tree
                return newListEntry;
            };

            // Set up bind function for a specific list entry
            stateList.bindItem = (item, index) =>
            {
                (item.userData as StateEntryController).SetData(_state[index]);
            };

            // Set a fixed item height
            stateList.fixedItemHeight = 45;
        }


    }

}