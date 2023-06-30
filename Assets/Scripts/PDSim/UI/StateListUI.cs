using PDSim.Components;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace PDSim.UI
{
    public class StateListUI : MonoBehaviour
    {
        [SerializeField]
        VisualTreeAsset itemTemplate;

        private Label objectName;
        private ListView _stateList;
        private StateListController _stateListController;
        private VisualElement _root;

        private void OnEnable()
        {
            _root = GetComponent<UIDocument>().rootVisualElement;

            _stateListController = new StateListController();


            _stateList = _root.Q<ListView>("StateList");

            objectName = _root.Q<Label>("ObjectName");
        }

        public StateListController InitializePlanList(string name, List<PdBooleanPredicate> list)
        {
            objectName.text = name;
            _stateListController.InitializeStateList(_root, itemTemplate);
            _stateListController.SetPlanActions(list);
            return _stateListController;
        }

        public void Clear()
        {
            objectName.text = "";
            _stateListController.ClearList();
        }

        public void ToggleVisibility()
        {
            _root.visible = !_root.visible;
        }

        public void HighlightCurrentAction(int index)
        {
            _stateList.AddToSelection(index);
        }
    }
}
