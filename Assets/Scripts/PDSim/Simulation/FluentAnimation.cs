using PDSim.Components;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace PDSim.Simulation
{
    [System.Serializable]
    public class FluentAnimation : MonoBehaviour
    {
        public string fluentName;
        public List<FluentAnimationData> data;

        public FluentAnimation(PdTypedPredicate fluent)
        {
            fluentName = fluent.name;
            data = new List<FluentAnimationData>();
        }
    }


    [System.Serializable]
    public class FluentAnimationData
    {
        public string name;
        public ScriptMachine machine;
        public int order;
    }

}