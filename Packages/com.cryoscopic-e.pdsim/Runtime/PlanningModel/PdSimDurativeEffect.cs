using System;
using System.Collections.Generic;

namespace PDSim.PlanningModel
{
    
    /// <summary>
    /// Durative effect. 
    /// </summary>
    [Serializable]
    public class PdSimDurativeEffect : PdSimEffect
    {
        public EffectTiming timing;
        public PdSimDurativeEffect(Effect effect, List<PdSimParameter> parameters) : base(effect, parameters)
        {
            timing = new EffectTiming(effect.OccurrenceTime);
        }

        public override string ToString()
        {
            string effect = "";

            effect += timing.ToString();

            if (effectKind == EffetKind.Increase || effectKind == EffetKind.Decrease)
            {
                effect += $" |{effectKind.ToString().ToUpper()}| ";
            }

            effect += fluentAssignment.ToString();

            effect += "\n";

            return effect;
        }
    }

     /// <summary>
    /// Class that represents the timing of a durative effect.
    /// e.g. AtStart, AtEnd, OverAll, AtTime
    /// </summary>
    [Serializable]
    public class EffectTiming
    {
        public float delay;
        public string timepoint;
        public EffectTiming(Timing effectDuration)
        {
            delay = PdSimAtom.RealToFloat(effectDuration.Delay);
            timepoint = effectDuration.Timepoint.Kind.ToString();
        }

        public override string ToString()
        {
            if (delay > 0)
                return $"|{timepoint} <> Delay: {delay}|";
            else
                return $"|{timepoint}| ";
        }
    }
}