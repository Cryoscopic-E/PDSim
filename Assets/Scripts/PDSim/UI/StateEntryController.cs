using PDSim.Components;
using UnityEngine.UIElements;

public class StateEntryController
{
    Label label;


    public void SetVisualElement(VisualElement visualElement)
    {
        label = visualElement.Q<Label>("Item");
    }


    public void SetData(PdBooleanPredicate data)
    {
        label.text = data.ToString();
    }
}

