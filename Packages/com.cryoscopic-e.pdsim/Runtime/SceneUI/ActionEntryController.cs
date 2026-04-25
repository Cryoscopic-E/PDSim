using GeTPlan.Core.Models;
using UnityEngine.UIElements;

namespace PDSim.SceneUI
{
    /// <summary>
    /// Controller for an individual action entry in the Plan List.
    /// Manages the display of a grounded action's string representation.
    /// </summary>
    public class ActionEntryController
    {
        #region Private Fields
        private Label _label;
        #endregion

        #region Public Methods
        /// <summary>
        /// Sets the visual element (Label) to be managed by this controller.
        /// </summary>
        /// <param name="element">The Label visual element.</param>
        public void SetVisualElement(Label element)
        {
            _label = element;
        }

        /// <summary>
        /// Sets the action data for this entry.
        /// </summary>
        /// <param name="data">The grounded action data.</param>
        public void SetActionData(GroundedAction data)
        {
            _label.text = data.ToString();
        }
        #endregion
    }
}
