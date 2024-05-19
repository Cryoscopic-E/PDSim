using PDSim.Simulation.SimulationObject;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

namespace PDSim.VisualScripting.Animations
{
    [UnitCategory("PDSim/Animations")]
    public class TranslateToObject : WaitUnit
    {

        [DoNotSerialize]
        public ValueInput movingObject;

        [DoNotSerialize]
        public ValueInput movingObjectOffset;

        [DoNotSerialize]
        public ValueInput targetObject;

        [DoNotSerialize]
        public ValueInput targetObjectOffset;

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
            var targetObj = flow.GetValue<GameObject>(targetObject);
            var movingObjOffset = flow.GetValue<Transform>(movingObjectOffset);
            var targetObjOffset = flow.GetValue<Transform>(targetObjectOffset);


            var duration = flow.GetValue<float>(this.duration);

            var timer = 0f;

            var startPosition = movingObj.transform.position;
            var goal = targetObj.transform.position;

            // offset the goal position
            if (movingObjOffset != null)
            {
                goal -= movingObjOffset.localPosition;
            }
            if (targetObjOffset != null)
            {
                goal += targetObjOffset.localPosition;
            }

            // if moving object is nav agent, set destination
            var simObj = movingObj.GetComponent<PdSimSimulationObject>();
            if (simObj.useNavMeshAgent)
            {
                // set the nav agent destination
                yield return simObj.MoveTo(goal);
                movingObj.transform.position = goal;
            }
            else
            {
                // if the duration is 0, just set the position and return
                if (duration == 0.0f)
                {
                    resultValue = goal;
                    movingObj.transform.position = goal;
                    yield return null;
                    yield return exit;
                    yield break;
                }

                // if MovementSettings is set, use those values
                if (simObj.movementSettings != null)
                {
                    yield return simObj.MoveTo(goal);
                    movingObj.transform.position = goal;
                    yield return null;
                    yield return exit;
                    yield break;
                }



                // if the height should be matched first
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
                    // Lerp the position for the duration
                    for (float progress = 0; progress < duration; progress += Time.deltaTime)
                    {
                        movingObj.transform.position = Vector3.Lerp(startPosition, goal, progress / duration);
                        yield return null;
                    }
                }
            }
            resultValue = goal;
            yield return exit;
        }

        protected override void Definition()
        {
            base.Definition();

            movingObject = ValueInput<GameObject>("Moving Object");
            movingObjectOffset = ValueInput<Transform>("Moving obj Offset", null); ;

            targetObject = ValueInput<GameObject>("Target Object");
            targetObjectOffset = ValueInput<Transform>("Target obj Offset", null);

            matchHeightFirst = ValueInput<bool>("Match Height First");
            matchHeightFirst.SetDefaultValue(false);

            duration = ValueInput<float>("Duration");
            duration.SetDefaultValue(1f);

            result = ValueOutput<Vector3>("End Position", (flow) => resultValue);

            Requirement(movingObject, enter);
            Requirement(targetObject, enter);
            Assignment(enter, result);
        }


    }

}