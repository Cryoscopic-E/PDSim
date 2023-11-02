using PDSim.Protobuf;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace PDSim.Simulation
{
    [System.Serializable]
    public class FluentAnimation : MonoBehaviour
    {
        public PdSimFluent metaData;
        public List<FluentAnimationData> animationData;

        public bool AddAnimationData(ScriptMachine machine, int order = 0)
        {
            // Check if same name is in list
            foreach (var data in animationData)
            {
                if (data.name == machine.name)
                {
                    return false;
                }
            }


            animationData.Add(new FluentAnimationData()
            {
                name = machine.name,
                machine = machine
            });
            return true;
        }

    }


    [System.Serializable]
    public class FluentAnimationData
    {
        public string name;
        public ScriptMachine machine;
    }
}