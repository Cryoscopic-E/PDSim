using System;
using System.Collections.Generic;

namespace PDSim.PlanningModel
{
    /// <summary>
    /// Class that represents an action instance in the problem.
    /// e.g. (move truck1 location1 location2) as in a plan.
    /// </summary>
    [Serializable]
    public class PdSimActionInstance
    {
        /// <summary>
        /// Id of the action instance.
        /// </summary> 
        public string id;
        /// <summary>
        /// Action name.
        /// </summary>
        public string name;
        /// <summary>
        /// List of objects that are parameters of the action.
        /// </summary>
        public List<string> parameters;
        public float startTime;
        public float endTime;

        public PdSimActionInstance(ActionInstance actionInstance)
        {
            id = actionInstance.Id;

            name = actionInstance.ActionName;

            parameters = new List<string>();
            foreach (var parameter in actionInstance.Parameters)
            {
                parameters.Add(parameter.Symbol);
            }

            startTime = -1f;
            endTime = -1f;

            if (actionInstance.StartTime != null)
            {
                startTime = PdSimAtom.RealToFloat(actionInstance.StartTime);
                endTime = PdSimAtom.RealToFloat(actionInstance.EndTime);
            }
        }

        public override string ToString()
        {
            // Action instance in format: Name (Parameter1, Parameter2, ...)
            string actionInstance = string.Format("{0} (", name);
            foreach (var parameter in parameters)
            {
                actionInstance += string.Format("{0}, ", parameter.ToString());
            }
            actionInstance = actionInstance.Remove(actionInstance.Length - 2);
            actionInstance += ")";

            if (startTime >= 0)
                actionInstance += $" |{startTime}, {endTime}|";

            return actionInstance;
        }
    }
}