using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using PDSim.Simulation;
using PDSim.Simulation.SimulationObject;

namespace PDSim.VisualScripting.Getters
{
    /// <summary>
    /// Get an object by name
    /// Using the Simulation Manager instance.
    /// </summary>
    [UnitCategory("PDSim/Getters")]
    public class GetObjectByName : Unit
    {
        [DoNotSerialize]
        public ControlInput inputTrigger;

        [DoNotSerialize]
        public ControlOutput outputTrigger;


        [DoNotSerialize]
        public ValueInput objectName;

        [DoNotSerialize]
        public ValueOutput simulationObject;

        private PdSimSimulationObject simulationObjectValue;

        protected override void Definition()
        {
            inputTrigger = ControlInput("inputTrigger", GetSimObject);
            outputTrigger = ControlOutput("outputTrigger");

            objectName = ValueInput<string>("Object Name", null);
            simulationObject = ValueOutput<PdSimSimulationObject>("Simulation Object", (flow) => { return simulationObjectValue; });

            Succession(inputTrigger, outputTrigger);
            Requirement(objectName, inputTrigger);
        }

        private ControlOutput GetSimObject(Flow flow)
        {
            var objectName = flow.GetValue<string>(this.objectName);

            if (objectName == null)
            {
                Debug.LogError("No Object Name");
                return outputTrigger;
            }

            simulationObjectValue = PdSimManager.Instance.GetSimulationObject(objectName);

            return outputTrigger;
        }
    }
}

