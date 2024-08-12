using PDSim.Protobuf;
using PDSim.Simulation;
using UnityEngine.UIElements;

namespace PDSim.Editor.EditorUI
{
    public class PredicateAnimationAttributeController
    {
        private VisualElement _visualElement;

        private DropdownField _attribute;

        private PdSimParameter _metadata;

        public void SetMetadata(PdSimParameter metadata)
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
            _attribute.choices = PdSimManager.Instance.problemModel.typesDeclaration.GetChildrenTypes(_metadata.type);
            _attribute.value = _attribute.choices[0];

        }
    }
}