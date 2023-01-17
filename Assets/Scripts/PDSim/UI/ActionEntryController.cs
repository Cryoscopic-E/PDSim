using PDSim.Types;
using UnityEngine.UIElements;

public class ActionEntryController
{
    Label label;


    public void SetVisualElement(VisualElement visualElement)
    {
        label = visualElement.Q<Label>("ActionItem");
    }


    public void SetActionData(PDAction data)
    {
        label.text = data.ToString();
    }
}

