using System;
using System.Collections.Generic;

namespace PDSim.PlanningModel
{
    
    /// <summary>
    /// Base class that represents an effect in the action.
    /// </summary>
    [Serializable]
    public class PdSimEffect
    {
        public enum EffetKind
        {
            None = 0,
            Assignment,
            Increase,
            Decrease
        }

        public PdSimFluentAssignment fluentAssignment;
        public List<int> actionParametersMap;
        public EffetKind effectKind;
        public PdSimEffect(Effect effect, List<PdSimParameter> actionParameters)
        {
            var e = effect.Effect_;
            var value = new PdSimAtom(e.Value.Atom);

            // kind of effect
            switch (e.Kind)
            {
                default:
                    effectKind = EffetKind.None;
                    break;
                case EffectExpression.Types.EffectKind.Assign:
                    effectKind = EffetKind.Assignment;
                    break;
                case EffectExpression.Types.EffectKind.Increase:
                    effectKind = EffetKind.Increase;
                    break;
                case EffectExpression.Types.EffectKind.Decrease:
                    effectKind = EffetKind.Decrease;
                    break;
            }


            var fluent = e.Fluent.List;
            var fluentName = fluent[0].Atom.Symbol;
            var parameters = new List<string>();
            actionParametersMap = new List<int>();

            for (int i = 1; i < fluent.Count; i++)
            {
                var parameter = fluent[i].Atom.Symbol;
                parameters.Add(parameter);

                // if parameter is in action parameters, add index
                var index = actionParameters.FindIndex(p => p.name == parameter);
                actionParametersMap.Add(index);

            }
            fluentAssignment = new PdSimFluentAssignment(value, fluentName, parameters);


        }
        public override string ToString()
        {
            string effect = "";

            if (effectKind != EffetKind.None)
                effect += $"|{effectKind.ToString().ToUpper()}| ";

            effect += fluentAssignment.ToString();

            effect += "\n";

            return effect;
        }
    }
}