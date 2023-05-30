using Unity.VisualScripting;

namespace PDSim.Animation
{
    [UnitCategory("PDSim/SimulationObjects")]
    public class GetSimObjectPoint : Unit
    {
        //[DoNotSerialize]
        //[PortLabelHidden]
        //public ValueInput @simObject;

        //[DoNotSerialize]
        //public ValueInput pointName;

        //[DoNotSerialize]
        //[PortLabelHidden]
        //public ValueOutput point;

        protected override void Definition()
        {
            //@simObject = ValueInput<GameObject>(nameof(@simObject), null);

            //pointName = ValueInput(nameof(pointName), string.Empty);

            //point = ValueOutput<Transform>(nameof(point), Get).Predictable();

            //Requirement(pointName, point);
            //Requirement(@simObject, point);
        }
        //private Transform Get(Flow flow)
        //{
        //    var name = flow.GetValue<string>(this.pointName);

        //    GameObject simulationObject = null;

        //    simulationObject = flow.GetValue<GameObject>(@simObject);

        //    var pdsimObject = simulationObject.GetComponent<PdSimSimulationObject>();

        //    return pdsimObject.SimulationObjectPoints.GetPoint(name);
        //}

        //// Event handler to set the PointStorage input
        //private void SetPointStorage(Flow flow, PdSimSimulationObject obj)
        //{
        //    simObject = ValueInput<PdSimSimulationObject>("simObject", obj);
        //}

        //// Event handler to set the selected transform name
        //private void SetTransformName(Flow flow, string transformName)
        //{
        //    pointName = ValueInput<string>("pointName", transformName);
        //}
    }
}