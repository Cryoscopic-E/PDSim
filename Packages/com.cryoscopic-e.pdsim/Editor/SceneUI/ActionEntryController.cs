using PDSim.Protobuf;
using UnityEngine.UIElements;

namespace PDSim.Editor.SceneUI
{
    public class ActionEntryController
    {
        Label label;

        public void SetVisualElement(VisualElement visualElement)
        {
            label = visualElement.Q<Label>("Item");
        }


        public void SetActionData(PdSimActionInstance data)
        {
            label.text = data.ToString();
        }
    }
}
