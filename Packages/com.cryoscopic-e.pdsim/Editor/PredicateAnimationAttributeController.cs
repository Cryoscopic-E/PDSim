using PDSim.Components;
using UnityEngine.UIElements;
using System.Collections.Generic;

namespace PDSim.Editor
{
    /// <summary>
    /// Controller for a single attribute row in the CreateAnimationWindow.
    /// </summary>
    public class PredicateAnimationAttributeController
    {
        #region Fields
        private VisualElement _visualElement;
        private DropdownField _attribute;
        private string _attributeName;
        private string _attributeType;
        #endregion

        #region Public Methods
        /// <summary>
        /// Sets the metadata for this attribute row.
        /// </summary>
        /// <param name="attributeName">The name of the attribute.</param>
        /// <param name="attributeType">The PDDL type of the attribute.</param>
        public void SetMetadata(string attributeName, string attributeType)
        {
            _attributeName = attributeName;
            _attributeType = attributeType;
        }

        /// <summary>
        /// Sets the visual element that this controller manages.
        /// </summary>
        /// <param name="visualElement">The visual element.</param>
        public void SetVisualElement(VisualElement visualElement)
        {
            _visualElement = visualElement;
        }

        /// <summary>
        /// Gets the current value of the attribute dropdown.
        /// </summary>
        /// <returns>The selected attribute type.</returns>
        public string GetValue()
        {
            return _attribute.value;
        }

        /// <summary>
        /// Updates the UI content of the managed visual element.
        /// </summary>
        public void UpdateContent()
        {
            var root = _visualElement;

            _attribute = root.Q<DropdownField>("Attribute");
            _attribute.label = "?" + _attributeName;
            
            if (TypeHierarchy.Instance != null && TypeHierarchy.Instance.ModelTypes != null)
            {
                _attribute.choices = TypeHierarchy.Instance.ModelTypes.GetChildrenTypes(_attributeType);
                if (_attribute.choices.Count > 0)
                {
                    _attribute.value = _attribute.choices[0];
                }
                else
                {
                    _attribute.choices = new List<string> { _attributeType };
                    _attribute.value = _attributeType;
                }
            }
            else
            {
                _attribute.choices = new List<string> { _attributeType };
                _attribute.value = _attributeType;
            }
        }
        #endregion
    }
}