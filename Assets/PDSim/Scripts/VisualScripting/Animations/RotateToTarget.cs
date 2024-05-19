// Animation script that rotates the object to face the target object

using System.Collections;
using Newtonsoft.Json;
using Unity.VisualScripting;
using UnityEngine;

namespace PDSim.VisualScripting.Animations
{
    [UnitCategory("PDSim/Animations")]
    public class RotateToTarget : WaitUnit
    {
        [DoNotSerialize]
        public ValueInput movingObject;

        [DoNotSerialize]
        public ValueInput targetObject;

        [DoNotSerialize]
        public ValueInput duration;

        protected override void Definition()
        {
            base.Definition();
            movingObject = ValueInput<GameObject>("movingObject");
            targetObject = ValueInput<GameObject>("targetObject");
            duration = ValueInput<float>("duration");
            duration.SetDefaultValue(1f);
            Requirement(movingObject, enter);
            Requirement(targetObject, enter);

            Succession(enter, exit);
        }

        protected override IEnumerator Await(Flow flow)
        {
            // Get the values from the ports
            var movingObj = flow.GetValue<GameObject>(movingObject);
            var targetObj = flow.GetValue<GameObject>(targetObject);
            var duration = flow.GetValue<float>(this.duration);

            var timer = 0f;

            var startPosition = movingObj.transform.position;
            var goal = targetObj.transform.position;

            while (timer < duration)
            {
                timer += Time.deltaTime;
                movingObj.transform.rotation = Quaternion.Slerp(movingObj.transform.rotation, Quaternion.LookRotation(goal - movingObj.transform.position), timer / duration);
                yield return null;
            }

            movingObj.transform.rotation = Quaternion.LookRotation(goal - movingObj.transform.position);

            yield return null;
            yield return exit;
        }
    }
}