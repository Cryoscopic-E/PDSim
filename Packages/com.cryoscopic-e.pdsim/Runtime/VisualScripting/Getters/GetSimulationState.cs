using PDSim.Protobuf;
using PDSim.Simulation;
using Unity.VisualScripting;

namespace PDSim.VisualScripting.Getters
{
    /// <summary>
    /// Get current simulating action informations
    /// </summary>
    [UnitCategory("PDSim/Getters")]
    public class GetSimulationState : Unit
    {
        [DoNotSerialize]
        public ControlInput inputTrigger;

        [DoNotSerialize]
        public ControlOutput outputTrigger;


        [DoNotSerialize]
        public ValueOutput temporalProblem;

        [DoNotSerialize]
        public ValueOutput simulatingPlan;

        [DoNotSerialize]
        public ValueOutput currentAction;


        private SimulationState simulationState;

        protected override void Definition()
        {
            inputTrigger = ControlInput("inputTrigger", GetActionInstance);
            outputTrigger = ControlOutput("outputTrigger");

            temporalProblem = ValueOutput<bool>("Is Timed Problem", (flow) => simulationState.isTimedProblem);
            simulatingPlan = ValueOutput<bool>("Is Simulating Plan", (flow) => simulationState.simulatingPlan);
            currentAction = ValueOutput<PdSimActionInstance>("Current Action", (flow) => simulationState.currentAction);

            Succession(inputTrigger, outputTrigger);
        }

        private ControlOutput GetActionInstance(Flow flow)
        {
            simulationState = PdSimManager.Instance.SimulationState;

            return outputTrigger;
        }
    }
}

