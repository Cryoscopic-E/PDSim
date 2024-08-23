using System;
using System.Collections.Generic;

namespace PDSim.PlanningModel
{
    /// <summary>
    /// Durative action.
    /// e.g. Temporal planning actions.
    /// </summary>
    [Serializable]
    public class PdSimDurativeAction : PdSimAction
    {
        public List<PdSimDurativeEffect> effects;
        public PdSimDuration duration;
        public PdSimDurativeAction(Action action) : base(action)
        {
            effects = new List<PdSimDurativeEffect>();
            foreach (var effect in action.Effects)
            {
                effects.Add(new PdSimDurativeEffect(effect, parameters));
            }
            duration = new PdSimDuration(action.Duration);
        }
    }

    /// <summary>
    /// Class that represents a duration of a durative action with upper and lower bounds.
    /// e.g. [0, 10]
    /// </summary>
    [Serializable]
    public class PdSimDuration
    {
        public float upperBound;
        public float lowerBound;

        public PdSimDuration(Duration duration)
        {
            var atomUpperBound = duration.ControllableInBounds.Upper.Atom;
            var atomLowerBound = duration.ControllableInBounds.Lower.Atom;

            if (atomUpperBound == null || atomLowerBound == null)
            {
                upperBound = 0f;
                lowerBound = 0f;
            }
            else
            {

                // check if atom is int or real
                if (atomUpperBound.ContentCase == Atom.ContentOneofCase.Int)
                {
                    upperBound = atomUpperBound.Int;
                }

                if (atomLowerBound.ContentCase == Atom.ContentOneofCase.Int)
                {
                    lowerBound = atomLowerBound.Int;
                }

                if (atomUpperBound.ContentCase == Atom.ContentOneofCase.Real)
                {
                    upperBound = PdSimAtom.RealToFloat(atomUpperBound.Real);
                }

                if (atomLowerBound.ContentCase == Atom.ContentOneofCase.Real)
                {
                    lowerBound = PdSimAtom.RealToFloat(atomLowerBound.Real);
                }
            }

        }

        public override string ToString()
        {
            if (lowerBound == upperBound)
                return $"[{lowerBound}]";
            else
                return $"[{lowerBound}, {upperBound}]";
        }

    }
}