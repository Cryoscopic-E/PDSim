using PDSim.Components;
using UnityEngine.UIElements;

namespace PDSim.Editor
{
    public class PredicateAnimationAttributeController
    {
        private VisualElement _visualElement;

        private DropdownField _attribute;

        private string _attributeName;
        private string _attributeType;

        public void SetMetadata(string attributeName, string attributeType)
        {
            _attributeName = attributeName;
            _attributeType = attributeType;
        }

        public void SetVisualElement(VisualElement visualElement)
        {
            _visualElement = visualElement;
        }

        public string GetValue()
        {
            return _attribute.value;
        }

        public void UpdateContent()
        {
            var root = _visualElement;

            _attribute = root.Q<DropdownField>("Attribute");
            _attribute.label = "?" + _attributeName;
            _attribute.choices = TypeHierarchy.Instance.modelTypes.GetChildrenTypes(_attributeType);
            _attribute.value = _attribute.choices[0];

        }
    }
}