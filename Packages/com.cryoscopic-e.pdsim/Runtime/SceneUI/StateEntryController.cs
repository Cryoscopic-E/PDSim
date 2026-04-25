using GeTPlan.Core.Models.Expressions;
using UnityEngine.UIElements;

namespace PDSim.SceneUI
{
    /// <summary>
    /// Controller for an individual state entry in the State List.
    /// Manages the display of a fluent and its value.
    /// </summary>
    public class StateEntryController
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
        /// Sets the data for this state entry.
        /// </summary>
        /// <param name="data">A tuple containing the fluent expression and its current value.</param>
        public void SetData((FluentExpression Fluent, object Value) data)
        {
            _label.text = data.ToString();
        }
        #endregion
    }
}
