using PDSim.Protobuf;
using UnityEngine.UIElements;

namespace PDSim.UI
{
    public class StateEntryController
    {
        Label label;


        public void SetVisualElement(VisualElement visualElement)
        {
            label = visualElement.Q<Label>("Item");
        }


        public void SetData(PdSimFluentAssignment data)
        {
            label.text = data.ToString();
        }
    }

}