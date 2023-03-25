using System.Collections.Generic;
using System.Linq;
using PDSim.Simulation;
using Unity.VisualScripting;
using UnityEngine;

namespace PDSim.Animation
{
    public class GetSimObjectPoint : Unit
    {
        [DoNotSerialize] 
        [PortLabelHidden] 
        public ValueInput simObject;
        
        [DoNotSerialize]
        [PortLabelHidden]
        public ValueInput pointName;
        
        [DoNotSerialize]
        [PortLabelHidden]
        public ValueOutput point;
       
        
        private List<Transform> _pointsList;
        private List<string> _pointNames;
        
        
        
        
        protected override void Definition()
        {
            simObject = ValueInput<PdSimSimulationObject>("SimObject");
            pointName = ValueInput<string>("PointName");
            point = ValueOutput("Point", GetPoint);
        }
        
        private Vector3 GetPoint(Flow flow)
        {
            var name = flow.GetValue<string>(this.pointName);
            var simObj = flow.GetValue<PdSimSimulationObject>(simObject);
            _pointsList = simObj.GetPoints();
            return _pointsList.FirstOrDefault(p => p.name == name)!.localPosition;
        }
        
        
    }
}