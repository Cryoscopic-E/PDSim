using GeTModel;
using UnityEngine.UIElements;

namespace PDSim.SceneUI
{
    public class StateEntryController
    {
        Label label;


        public void SetVisualElement(VisualElement visualElement)
        {
            label = visualElement.Q<Label>("Item");
        }


        public void SetData(GeTStateVariable data)
        {
            label.text = data.ToString();
        }
    }

}