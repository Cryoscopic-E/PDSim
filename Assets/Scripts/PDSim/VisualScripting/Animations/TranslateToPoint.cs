using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

namespace PDSim.VisualScripting.Animations
{
    /// <summary>
    /// Animation unity coroutine for translating an object to a point.
    /// </summary>
    [UnitCategory("PDSim/Animations")]
    public class TranslateToPoint : WaitUnit
    {

        [DoNotSerialize]
        public ValueInput movingObject;

        [DoNotSerialize]
        public ValueInput movingObjectOffset;

        [DoNotSerialize]
        public ValueInput targetPoint;

        [DoNotSerialize]
        public ValueInput matchHeightFirst;

        [DoNotSerialize]
        public ValueInput duration;

        [DoNotSerialize]
        public ValueOutput result;

        private Vector3 resultValue;

        protected override IEnumerator Await(Flow flow)
        {
            // Get the values from the ports
            var movingObj = flow.GetValue<GameObject>(movingObject);
            var movingObjOffset = flow.GetValue<Transform>(movingObjectOffset);
            var tPoint = flow.GetValue<Vector3>(targetPoint);

            var duration = flow.GetValue<float>(this.duration);

            var timer = 0f;

            var startPosition = movingObj.transform.position;
            var goal = tPoint;

            if (movingObjOffset != null)
            {
                goal -= movingObjOffset.localPosition;
            }

            if (duration == 0)
            {
                movingObj.transform.position = goal;
                resultValue = goal;
                yield return exit;
            }


            if (flow.GetValue<bool>(matchHeightFirst))
            {
                // align height first for a third of the duration
                while (timer < duration / 3)
                {
                    timer += Time.deltaTime;

                    movingObj.transform.position = Vector3.Lerp(startPosition, new Vector3(movingObj.transform.position.x, goal.y, movingObj.transform.position.z), timer / (duration / 3));

                    yield return null;
                }

                startPosition = movingObj.transform.position;

                // align x and z for the remaining duration
                while (timer < duration)
                {
                    timer += Time.deltaTime;

                    movingObj.transform.position = Vector3.Lerp(startPosition, goal, (timer - (duration / 3)) / (duration - (duration / 3)));

                    yield return null;
                }
            }
            else
            {

                for (float progress = 0; progress < duration; progress += Time.deltaTime)
                {
                    movingObj.transform.position = Vector3.Lerp(startPosition, goal, progress / duration);
                    yield return null;
                }
            }


            resultValue = goal;
            yield return exit;
        }

        protected override void Definition()
        {
            base.Definition();

            movingObject = ValueInput<GameObject>("Moving Object");
            movingObjectOffset = ValueInput<Transform>("Moving obj Offset", null);

            targetPoint = ValueInput<Vector3>("Target");

            matchHeightFirst = ValueInput<bool>("Match Height First");
            matchHeightFirst.SetDefaultValue(false);

            duration = ValueInput<float>("Duration");
            duration.SetDefaultValue(1f);

            result = ValueOutput<Vector3>("End Position", (flow) => resultValue);

            Requirement(movingObject, enter);
            Requirement(targetPoint, enter);

            // Data is written to the result string output when the inputTrigger is triggered.
            Assignment(enter, result);
        }


    }

}