using System;

namespace PDSim.PlanningModel
{
    /// <summary>
    /// Class representing an object in the domain problem.
    /// </summary>

    [Serializable]
    public class PdSimObject
    {
        public string name;
        public string type;

        public PdSimObject(string name, string type)
        {
            this.name = name;
            this.type = type;
        }

        public override string ToString()
        {
            return string.Format("{0} - {1}", name, type);
        }
    }

}