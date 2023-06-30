using System.Collections.Generic;
using PDSim.Components;
using UnityEngine;
using UnityEngine.UIElements;

namespace Editor.UI
{
    public class PredicateAnimationAttributeController
    {
        private VisualElement _visualElement;

        private DropdownField _attribute;

        private PdObject _metadata;

        public void SetMetadata(PdObject metadata)
        {
            _metadata = metadata;
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
            _attribute.label = "?" + _metadata.name;
            _attribute.choices = _metadata.childrenTypes;
            _attribute.value = _attribute.choices[0];

        }
    }
}