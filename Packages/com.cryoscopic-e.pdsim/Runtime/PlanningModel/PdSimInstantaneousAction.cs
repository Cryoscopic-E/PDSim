using System;
using System.Collections.Generic;

namespace PDSim.PlanningModel
{
    /// <summary>
    /// Instantaneous action.
    /// e.g. Classical planning actions.
    /// </summary>
    [Serializable]
    public class PdSimInstantaneousAction : PdSimAction
    {
        /// <summary>
        /// List of effects of the action.
        /// </summary>
        public List<PdSimEffect> effects;
        public PdSimInstantaneousAction(Action action) : base(action)
        {
            effects = new List<PdSimEffect>();
            foreach (var effect in action.Effects)
            {
                effects.Add(new PdSimEffect(effect, parameters));
            }
        }
    }
}
