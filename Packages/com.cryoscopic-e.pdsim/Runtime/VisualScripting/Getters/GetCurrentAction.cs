using System.Collections.Generic;
using Unity.VisualScripting;
using PDSim.PlanningModel;

namespace PDSim.VisualScripting.Getters
{
    /// <summary>
    /// Get current simulating action informations
    /// </summary>
    [UnitCategory("PDSim/Getters")]
    public class GetCurrentAction : Unit
    {
        [DoNotSerialize]
        public ControlInput inputTrigger;

        [DoNotSerialize]
        public ControlOutput outputTrigger;

        [DoNotSerialize]
        public ValueInput action;

        [DoNotSerialize]
        public ValueOutput actionName;

        [DoNotSerialize]
        public ValueOutput parameters;

        [DoNotSerialize]
        public ValueOutput startTime;

        [DoNotSerialize]
        public ValueOutput endTime;

        private PdSimActionInstance actionInstance;

        protected override void Definition()
        {
            inputTrigger = ControlInput("inputTrigger", GetActionInstance);
            outputTrigger = ControlOutput("outputTrigger");

            action = ValueInput<PdSimActionInstance>("Action Instance");
            actionName = ValueOutput<string>("Action Name", (flow) => { return actionInstance.name; });
            parameters = ValueOutput<List<string>>("Parameters", (flow) => { return actionInstance.parameters; });
            startTime = ValueOutput<float>("Start Time", (flow) => { return actionInstance.startTime; });
            endTime = ValueOutput<float>("End Time", (flow) => { return actionInstance.endTime; });

            Succession(inputTrigger, outputTrigger);
            Requirement(action, inputTrigger);
        }

        private ControlOutput GetActionInstance(Flow flow)
        {
            actionInstance = flow.GetValue<PdSimActionInstance>(action);

            return outputTrigger;
        }
    }
}
