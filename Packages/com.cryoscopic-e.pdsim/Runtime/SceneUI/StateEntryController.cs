using GeTPlan.Core.Models; using GeTPlan.Core.Logic; using GeTPlan.Core.Models.Expressions; using PDSimAPI;
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


        public void SetData((FluentExpression Fluent, object Value) data)
        {
            label.text = data.ToString();
        }
    }

}