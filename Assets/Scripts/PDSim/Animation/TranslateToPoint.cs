using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

namespace PDSim.Animation
{
    [UnitCategory("PDSim/Animation")]
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





        /// <summary>
        /// Object new Position
        /// </summary>
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
            var goal = tPoint - movingObjOffset.localPosition;


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
            movingObjectOffset = ValueInput<Transform>("Moving obj Offset");

            targetPoint = ValueInput<Vector3>("Target");

            matchHeightFirst = ValueInput<bool>("Match Height First");
            matchHeightFirst.SetDefaultValue(false);

            duration = ValueInput<float>("Duration");
            duration.SetDefaultValue(1f);

            result = ValueOutput<Vector3>("End Position", (flow) => resultValue);

            Requirement(movingObject, enter); //Specifies that we need the myValueA value to be set before the node can run.
            Requirement(targetPoint, enter); //Specifies that we need the myValueB value to be set before the node can run.
            //Succession(inputTrigger, outputTrigger); //Specifies that the input trigger port's input exits at the output trigger port. Not setting your succession also dims connected nodes, but the execution still completes.
            Assignment(enter, result);//Specifies that data is written to the result string output when the inputTrigger is triggered.
        }


    }

}