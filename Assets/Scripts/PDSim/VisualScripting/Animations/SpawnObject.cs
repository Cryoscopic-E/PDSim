using UnityEngine;
using Unity.VisualScripting;

namespace PDSim.Animations
{
    [UnitCategory("PDSim/Animations")]
    public class SpawnObject : Unit
    {
        [DoNotSerialize]
        public ControlInput inputTrigger;

        [DoNotSerialize]
        public ControlOutput outputTrigger;

        [DoNotSerialize]
        public ValueInput prefabObject;

        [DoNotSerialize]
        public ValueInput newObjectName;

        [DoNotSerialize]
        public ValueInput parentObject;

        [DoNotSerialize]
        public ValueInput objectPosition;

        [DoNotSerialize]
        public ValueInput objectRotation;


        protected override void Definition()
        {
            inputTrigger = ControlInput("inputTrigger", (flow) => { return Spawn(flow); });
            outputTrigger = ControlOutput("outputTrigger");
            
            prefabObject = ValueInput<GameObject>("prefabObject", null);
            newObjectName = ValueInput<string>("newObjectName", null);
            parentObject = ValueInput<GameObject>("parentObject", null);
            objectPosition = ValueInput<Vector3>("objectPosition", Vector3.zero);
            objectRotation = ValueInput<Vector3>("objectRotation", Quaternion.identity.eulerAngles);
            
            Succession(inputTrigger, outputTrigger);
            Requirement(prefabObject, inputTrigger);

        }

        private ControlOutput Spawn(Flow flow)
        {
            var clone = flow.GetValue<GameObject>(this.prefabObject);

            var newObjectName = flow.GetValue<string>(this.newObjectName);
            var parentObject = flow.GetValue<GameObject>(this.parentObject);
            var objectPosition = flow.GetValue<Vector3>(this.objectPosition);
            var objectRotation = flow.GetValue<Vector3>(this.objectRotation);


            var newObject = GameObject.Instantiate(clone, objectPosition, Quaternion.Euler(objectRotation));

            if (parentObject != null)
            {
                newObject.transform.parent = parentObject.transform;
            }

            if (newObjectName != null)
            {
                newObject.name = newObjectName;
            }


            return outputTrigger;
        }
    }
}
