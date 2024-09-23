using PDSim.PlanningModel;

namespace PDSim.Simulation
{
    /// <summary>
    /// Class to store animation data for a simulation object
    /// </summary>
    [System.Serializable]
    public class PdSimFluentAnimation : PdSimAnimation
    {
        public PdSimFluent metaData;
    }
}