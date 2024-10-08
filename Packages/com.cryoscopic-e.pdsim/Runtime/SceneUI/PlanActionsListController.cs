﻿using PDSim.PlanningModel;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace PDSim.SceneUI
{
    public class PlanActionsListController
    {
        VisualTreeAsset actionItemTemplate;

        ListView actionsList;


        public void InitializeActionList(VisualElement root, VisualTreeAsset labelTemplate)
        {
            actionItemTemplate = labelTemplate;

            actionsList = root.Q<ListView>("PlanList");
            FillActionsList();

        }

        private List<PdSimActionInstance> planActions;

        public void SetPlanActions(List<PdSimActionInstance> actions)
        {
            planActions = actions;
        }

        void FillActionsList()
        {
            // Set up a make item function for a list entry
            actionsList.makeItem = () =>
            {
                // Instantiate the UXML template for the entry
                var newListEntry = actionItemTemplate.Instantiate();

                // Instantiate a controller for the data
                var actionEntry = new ActionEntryController();

                // Assign the controller script to the visual element
                newListEntry.userData = actionEntry;

                // Initialize the controller script
                actionEntry.SetVisualElement(newListEntry);

                // Return the root of the instantiated visual tree
                return newListEntry;
            };

            // Set up bind function for a specific list entry
            actionsList.bindItem = (item, index) =>
            {
                (item.userData as ActionEntryController).SetActionData(planActions[index]);
            };

            // Set a fixed item height
            actionsList.fixedItemHeight = 45;

            // Set the actual item's source list/array
            actionsList.itemsSource = planActions;
        }
    }
}
