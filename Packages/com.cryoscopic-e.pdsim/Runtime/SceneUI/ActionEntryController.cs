using GeTPlan.Core.Models; using GeTPlan.Core.Logic; using GeTPlan.Core.Models.Expressions; using PDSimAPI;
using UnityEngine.UIElements;

namespace PDSim.SceneUI
{
    public class ActionEntryController
    {
        Label label;

        public void SetVisualElement(Label element)
        {
            label = element;
        }

        public void SetActionData(GroundedAction data)
        {
            label.text = data.ToString();
        }
    }
}
