using PDSim.Simulation;
using UnityEngine;
using UnityEngine.UIElements;

namespace PDSim.Editor.SceneUI
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

            _root.style.display = DisplayStyle.None;

            _stateListController = new StateListController();


            _stateList = _root.Q<ListView>("StateList");

            objectName = _root.Q<Label>("ObjectName");
        }

        public StateListController InitializeList(PdSimSimulationObject simObject)
        {
            objectName.text = simObject.name;
            _stateListController.InitializeStateList(_root, itemTemplate);
            _stateListController.SetState(simObject.GetObjectState());
            return _stateListController;
        }

        public void Clear()
        {
            objectName.text = "";
            _stateListController.ClearList();
        }

        public void ToggleVisibility()
        {
            _root.style.display = _root.style.display == DisplayStyle.None ? DisplayStyle.Flex : DisplayStyle.None;
        }

    }
}

