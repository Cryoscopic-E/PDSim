using System;
using System.Collections.Generic;

namespace PDSim.Protobuf
{
    /// <summary>
    /// Base class that represents an action in the domain.
    /// </summary>
    public abstract class PdSimAction
    {
        /// <summary>
        /// Action name.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Action parameters.
        /// e.g. (?t - truck, ?l1 - location, ?l2 - location)
        /// </summary>
        public List<PdSimParameter> Parameters { get; private set; }

        /// <summary>
        /// List of preconditions of the action.
        /// </summary>
        public List<PdSimCondition> Preconditions { get; private set; }

        /// <summary>
        /// List of effects of the action.
        /// </summary>
        public List<PdSimEffect> Effects { get; private set; }

        public PdSimAction(Action action)
        {
            Name = action.Name;

            Parameters = new List<PdSimParameter>();
            foreach (var parameter in action.Parameters)
            {
                Parameters.Add(new PdSimParameter(parameter));
            }

            Preconditions = new List<PdSimCondition>();

            Effects = new List<PdSimEffect>();

        }
    }
    /// <summary>
    /// Instantaneous action.
    /// e.g. Classical planning actions.
    /// </summary>
    [Serializable]
    public class PdSimInstantaneousAction : PdSimAction
    {
        public PdSimInstantaneousAction(Action action) : base(action)
        {
        }
    }

    [Serializable]
    public class PdSimDurativeAction : PdSimAction
    {
        public PdSimDuration Duration { get; private set; }

        public PdSimDurativeAction(Action action) : base(action)
        {
            Duration = new PdSimDuration(action.Duration);
        }
    }


    [Serializable]
    public class PdSimCondition
    {
        public PdSimCondition(Condition condition)
        {
        }
    }

    [Serializable]
    public class PdSimEffect
    {
        public PdSimEffect(Effect effect)
        {

        }
    }
    /// <summary>
    /// Class that represents an action instance in the problem.
    /// e.g. (move truck1 location1 location2) as in a plan.
    /// </summary>
    [Serializable]
    public class PdSimActionInstance
    {
        /// <summary>
        /// Id of the action instance.
        /// </summary> 
        public string Id { get; private set; }
        /// <summary>
        /// Action name.
        /// </summary>
        public string Name { get; private set; }
        /// <summary>
        /// List of objects that are parameters of the action.
        /// </summary>
        public List<string> Parameters { get; private set; }

        public float StartTime { get; private set; }

        public float EndTime { get; private set; }

        public PdSimActionInstance(ActionInstance actionInstance)
        {
            Id = actionInstance.Id;

            Name = actionInstance.ActionName;

            Parameters = new List<string>();
            foreach (var parameter in actionInstance.Parameters)
            {
                Parameters.Add(parameter.Symbol);
            }

            StartTime = -1f;
            EndTime = -1f;

            if (actionInstance.StartTime != null)
            {
                StartTime = PdSimUtils.RealToFloat(actionInstance.StartTime);
                EndTime = PdSimUtils.RealToFloat(actionInstance.EndTime);
            }
        }
    }

    [Serializable]
    public class PdSimDuration
    {
        public float UpperBound { get; private set; }

        public float LowerBound { get; private set; }

        public PdSimDuration(Duration duration)
        {
            var atomUpperBound = duration.ControllableInBounds.Upper.Atom;
            var atomLowerBound = duration.ControllableInBounds.Lower.Atom;

            // check if atom is int or real
            if (atomUpperBound.ContentCase == Atom.ContentOneofCase.Int)
            {
                UpperBound = atomUpperBound.Int;
            }

            if (atomLowerBound.ContentCase == Atom.ContentOneofCase.Int)
            {
                LowerBound = atomLowerBound.Int;
            }

            if (atomUpperBound.ContentCase == Atom.ContentOneofCase.Real)
            {
                UpperBound = PdSimUtils.RealToFloat(atomUpperBound.Real);
            }

            if (atomLowerBound.ContentCase == Atom.ContentOneofCase.Real)
            {
                LowerBound = PdSimUtils.RealToFloat(atomLowerBound.Real);
            }
        }

    }


}