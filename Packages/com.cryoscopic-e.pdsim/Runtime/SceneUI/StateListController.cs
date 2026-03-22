using GeTPlan.Core.Models; using GeTPlan.Core.Logic; using GeTPlan.Core.Models.Expressions; using PDSimAPI;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace PDSim.SceneUI
{
    public class StateListController
    {
        ListView stateList;
        private List<(FluentExpression Fluent, object Value)> _state = new();

        public void InitializeStateList(VisualElement root)
        {
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

        void SetupList()
        {
            stateList.makeItem = () =>
            {
                var label = new Label { name = "Item" };
                label.AddToClassList("list-item");

                var entry = new StateEntryController();
                label.userData = entry;
                entry.SetVisualElement(label);

                return label;
            };

            stateList.bindItem = (item, index) =>
            {
                (item.userData as StateEntryController).SetData(_state[index]);
            };

            stateList.fixedItemHeight = 45;
        }
    }
}
