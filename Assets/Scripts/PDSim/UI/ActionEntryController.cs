using PDSim.Components;
using UnityEngine.UIElements;

public class ActionEntryController
{
    Label label;


    public void SetVisualElement(VisualElement visualElement)
    {
        label = visualElement.Q<Label>("ActionItem");
    }


    public void SetActionData(PdPlanAction data)
    {
        label.text = data.ToString();
    }
}

