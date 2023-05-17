using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

namespace PDSim.Animation
{
    [UnitCategory("PDSim/Animation")]
    public class TranslateTo : WaitUnit
    {

        [DoNotSerialize]
        public ValueInput object0;

        [DoNotSerialize]
        public ValueInput offsetObject0;

        [DoNotSerialize]
        public ValueInput object1;

        [DoNotSerialize]
        public ValueInput offsetObject1;

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
            var obj0 = flow.GetValue<GameObject>(object0);
            var obj1 = flow.GetValue<GameObject>(object1);

            var duration = flow.GetValue<float>(this.duration);

            var t = 0f;

            var goal = obj1.transform.position;

            // run for duration in seconds
            while (t < duration)
            {
                // increment timer once per frame
                t += Time.deltaTime;

                // set position as a fraction of the distance between the markers.
                obj0.transform.position = Vector3.Lerp(obj0.transform.position, goal, t / duration);

                // return here next frame
                yield return null;
            }
            resultValue = obj1.transform.position;
            yield return null;
        }

        protected override void Definition()
        {
            base.Definition();

            object0 = ValueInput<GameObject>("object0");
            object1 = ValueInput<GameObject>("object1");
            duration = ValueInput<float>("duration");
            result = ValueOutput<Vector3>("Position", (flow) => resultValue);

            Requirement(object0, enter); //Specifies that we need the myValueA value to be set before the node can run.
            Requirement(object1, enter); //Specifies that we need the myValueB value to be set before the node can run.
            //Succession(inputTrigger, outputTrigger); //Specifies that the input trigger port's input exits at the output trigger port. Not setting your succession also dims connected nodes, but the execution still completes.
            Assignment(enter, result);//Specifies that data is written to the result string output when the inputTrigger is triggered.
        }


    }

}