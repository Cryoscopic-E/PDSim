using System;
using System.Collections.Generic;

namespace PDSim.Protobuf
{
    /// <summary>
    /// Class representing an atom.
    /// e.g 3, true, kitchen, at-robot
    /// </summary>
    [Serializable]
    public class PdSimAtom
    {
        public Atom.ContentOneofCase contentCase;

        public string valueSymbol;

        public PdSimAtom(Atom atom)
        {
            if (atom == null)
            {
                contentCase = Atom.ContentOneofCase.None;
                valueSymbol = string.Empty;
            }
            else
            {
                contentCase = atom.ContentCase;
                switch (contentCase)
                {
                    default:
                    case Atom.ContentOneofCase.Symbol:
                        valueSymbol = atom.Symbol;
                        break;
                    case Atom.ContentOneofCase.Int:
                        valueSymbol = atom.Int.ToString();
                        break;
                    case Atom.ContentOneofCase.Real:
                        valueSymbol = PdSimUtils.RealToFloat(atom.Real).ToString();
                        break;
                    case Atom.ContentOneofCase.Boolean:
                        valueSymbol = atom.Boolean.ToString();
                        break;
                }
            }
        }

        private PdSimAtom()
        {
            contentCase = Atom.ContentOneofCase.None;
            valueSymbol = string.Empty;
        }

        private PdSimAtom(string value)
        {
            if (value == "up:not") // then is a boolean
            {
                contentCase = Atom.ContentOneofCase.Boolean;
                valueSymbol = bool.FalseString;
            }
            else if (value == "up:and")
            {
                contentCase = Atom.ContentOneofCase.Symbol;
                valueSymbol = "AND";
            }
            else
            {
                // TODO: support other function symbols
                contentCase = Atom.ContentOneofCase.Symbol;
                valueSymbol = value;
            }
        }

        private PdSimAtom(int value)
        {
            contentCase = Atom.ContentOneofCase.Int;
            valueSymbol = value.ToString();
        }

        private PdSimAtom(float value)
        {
            contentCase = Atom.ContentOneofCase.Real;
            valueSymbol = value.ToString();
        }

        private PdSimAtom(bool value)
        {
            contentCase = Atom.ContentOneofCase.Boolean;
            valueSymbol = value.ToString();
        }

        public bool IsEmpty()
        {
            return contentCase == Atom.ContentOneofCase.None;
        }

        public override string ToString()
        {
            return valueSymbol;
        }

        public static PdSimAtom Empty()
        {
            return new PdSimAtom(new Atom());
        }

        public static PdSimAtom Boolean(bool value)
        {
            return new PdSimAtom(value);
        }

        public static PdSimAtom Symbol(string value)
        {
            return new PdSimAtom(value);
        }

        public bool IsTrue()
        {
            return contentCase == Atom.ContentOneofCase.Boolean && bool.Parse(valueSymbol) == true;
        }

        public bool IncreaseValue(float amount)
        {
            if (contentCase != Atom.ContentOneofCase.Real)
                return false;

            var value = float.Parse(valueSymbol);
            value += amount;
            valueSymbol = value.ToString();
            return true;
        }

        public bool DecreaseValue(float amount)
        {
            if (contentCase != Atom.ContentOneofCase.Real)
                return false;

            var value = float.Parse(valueSymbol);
            value -= amount;
            valueSymbol = value.ToString();
            return true;
        }
    }


    /// <summary>
    /// Class representing a parameter for a fluent, action or function.
    /// e.g. ?x - object
    /// </summary>
    [Serializable]
    public class PdSimParameter
    {
        public string name;
        public string type;

        public PdSimParameter(Parameter parameter)
        {
            name = parameter.Name;
            type = parameter.Type;
        }

        public PdSimParameter(string name, string type)
        {
            this.name = name;
            this.type = type;
        }

        public override string ToString()
        {
            return string.Format("{0} - {1}", name, type);
        }
    }

    /// <summary>
    /// Class representing a planning domain fluent.
    /// e.g. (at ?x ?y)
    /// </summary>

    [Serializable]
    public class PdSimFluent
    {
        public string name;
        public ValueType type;
        public List<PdSimParameter> parameters;

        public PdSimFluent(Fluent fluent)
        {
            name = fluent.Name;
            type = PdSimUtils.ConvertValueType(fluent.ValueType);
            parameters = new List<PdSimParameter>();
            foreach (var parameter in fluent.Parameters)
            {
                parameters.Add(new PdSimParameter(parameter));
            }
        }

        public override string ToString()
        {
            // Fluent in format: Type::Name (Parameter1 - Type, Parameter2 - Type, ...)
            string fluent = string.Format("{0}::{1}", type, name);

            if (parameters.Count > 0)
            {
                fluent += " (";
                foreach (var parameter in parameters)
                {
                    fluent += string.Format("{0}, ", parameter.ToString());
                }
                fluent = fluent.Remove(fluent.Length - 2);
                fluent += ")";
            }
            return fluent;
        }
    }

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

    /// <summary>
    /// Type of value of an Atom
    /// </summary>
    [Serializable]
    public enum ValueType
    {
        Symbol,
        Int,
        Real,
        Boolean
    }


    /// <summary>
    /// Base class that represents an action in the domain.
    /// </summary>
    [Serializable]
    public class PdSimAction
    {
        /// <summary>
        /// Action name.
        /// </summary>
        public string name;

        /// <summary>
        /// Action parameters.
        /// e.g. (?t - truck, ?l1 - location, ?l2 - location)
        /// </summary>
        public List<PdSimParameter> parameters;


        public PdSimAction(Action action)
        {
            name = action.Name;

            parameters = new List<PdSimParameter>();
            foreach (var parameter in action.Parameters)
            {
                parameters.Add(new PdSimParameter(parameter));
            }
        }

        public override string ToString()
        {
            // Action in format: Name (Parameter1 - Type, Parameter2 - Type, ...)
            string action = string.Format("{0} (", name);
            foreach (var parameter in parameters)
            {
                action += string.Format("{0}, ", parameter.ToString());
            }
            action = action.Remove(action.Length - 2);
            action += ")\n";

            return action;
        }
    }


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

    public enum EffetKind
    {
        None = 0,
        Assignment,
        Increase,
        Decrease
    }

    /// <summary>
    /// Base class that represents an effect in the action.
    /// </summary>
    [Serializable]
    public class PdSimEffect
    {

        public PdSimFluentAssignment fluentAssignment;
        public List<PdSimParameter> forAllVariables;
        public PdSimCondition effectCondition;
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

                // map parameter to action parameter
                for (int j = 0; j < actionParameters.Count; j++)
                {
                    if (parameter == actionParameters[j].name)
                    {
                        actionParametersMap.Add(j);
                        break;
                    }
                }
            }
            fluentAssignment = new PdSimFluentAssignment(value, fluentName, parameters);

            //forall
            forAllVariables = new List<PdSimParameter>();
            foreach (var parameter in e.Forall)
            {
                forAllVariables.Add(new PdSimParameter(parameter.Atom.Symbol, parameter.Type));
            }

            // conditional effect
            effectCondition = new PdSimCondition(e.Condition);
        }

        public bool IsForAll()
        {
            return forAllVariables.Count > 0;
        }

        public override string ToString()
        {
            string effect = "";
            if (IsForAll())
            {
                effect += $"FORALL ";
                foreach (var variable in forAllVariables)
                {
                    effect += string.Format("{0}, ", variable.ToString());
                }
                effect = effect.Remove(effect.Length - 2);
                effect += "\n";
            }

            effect += effectCondition.ToString();

            effect += fluentAssignment.ToString();

            effect += "\n";

            return effect;
        }
    }

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
            if (IsForAll())
            {
                effect += $"FORALL ";
                foreach (var variable in forAllVariables)
                {
                    effect += string.Format("{0}, ", variable.ToString());
                }
                effect = effect.Remove(effect.Length - 2);
                effect += "\n";
            }

            effect += effectCondition.ToString();

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
            delay = PdSimUtils.RealToFloat(effectDuration.Delay);
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


    /// <summary>
    /// Represents a condition in the action effect.
    /// </summary>
    [Serializable]
    public class PdSimCondition
    {
        public PdSimAtom functor;
        public List<PdSimFluentAssignment> assignments;

        public PdSimCondition(Expression expression)
        {
            functor = PdSimAtom.Empty();
            assignments = new List<PdSimFluentAssignment>();
            if (expression.Kind == ExpressionKind.StateVariable)
            {
                var expressionList = expression.List;
                var parameters = new List<string>();
                var fluentName = string.Empty;
                foreach (var exp in expressionList)
                {
                    if (exp.Kind == ExpressionKind.FluentSymbol)
                        fluentName = exp.Atom.Symbol;
                    else if (exp.Kind == ExpressionKind.Variable || exp.Kind == ExpressionKind.Parameter)
                    {
                        parameters.Add(exp.Atom.Symbol);
                    }
                }

                assignments.Add(new PdSimFluentAssignment(PdSimAtom.Boolean(true), fluentName, parameters));
            }
            else if (expression.Kind == ExpressionKind.FunctionApplication)
            {
                var firstAtom = expression.List[0].Atom;
                functor = PdSimAtom.Symbol(firstAtom.Symbol);
                // Case first element is a symbol not, then is a state variable
                if (firstAtom.Symbol == "up:not")
                {
                    var expressionStart = expression.List[1];
                    var parameters = new List<string>();
                    var fluentName = string.Empty;
                    foreach (var exp in expressionStart.List)
                    {
                        if (exp.Kind == ExpressionKind.FluentSymbol)
                            fluentName = exp.Atom.Symbol;
                        else if (exp.Kind == ExpressionKind.Variable || exp.Kind == ExpressionKind.Parameter)
                        {
                            parameters.Add(exp.Atom.Symbol);
                        }
                    }
                    assignments.Add(new PdSimFluentAssignment(PdSimAtom.Boolean(false), fluentName, parameters));
                }
                else // case functor is and, or, ..
                {
                    for (var i = 1; i < expression.List.Count; i++)
                    {
                        var expressionStart = expression.List[i];
                        var parameters = new List<string>();
                        var fluentName = string.Empty;
                        var value = PdSimAtom.Boolean(true); // default value is true


                        foreach (var exp in expressionStart.List)
                        {
                            if (exp.Kind == ExpressionKind.FluentSymbol)
                                fluentName = exp.Atom.Symbol;
                            else if (exp.Kind == ExpressionKind.FunctionSymbol)
                            {
                                value = PdSimAtom.Symbol(exp.Atom.Symbol);
                            }
                            else if (exp.Kind == ExpressionKind.StateVariable)
                            {
                                foreach (var exp2 in exp.List)
                                {
                                    if (exp2.Kind == ExpressionKind.FluentSymbol)
                                        fluentName = exp2.Atom.Symbol;
                                    else if (exp2.Kind == ExpressionKind.Variable || exp2.Kind == ExpressionKind.Parameter)
                                    {
                                        parameters.Add(exp2.Atom.Symbol);
                                    }
                                }
                            }
                            else // is a parameter or variable
                            {
                                parameters.Add(exp.Atom.Symbol);
                            }
                        }
                        assignments.Add(new PdSimFluentAssignment(value, fluentName, parameters));
                    }
                }
            }
        }

        public override string ToString()
        {
            string condition = string.Empty;

            if (assignments.Count > 0)
            {
                condition += $"IF\n";
                foreach (var assignment in assignments)
                {
                    condition += string.Format("{0} \n", assignment.ToString());
                }
                condition += "THEN\n";
            }

            return condition;
        }
    }

    /// <summary>
    /// Class that represents an assignment of a fluent.
    /// e.g. (at robot kitchen) := true
    /// </summary>
    [Serializable]
    public class PdSimFluentAssignment
    {
        public PdSimAtom value;
        public string fluentName;
        public List<string> parameters;

        public PdSimFluentAssignment(Assignment assignment)
        {
            value = new PdSimAtom(assignment.Value.Atom);
            var fluent = assignment.Fluent.List;
            // First element is the fluent name
            fluentName = fluent[0].Atom.Symbol;
            // Rest of the elements are the parameters
            parameters = new List<string>();
            for (int i = 1; i < fluent.Count; i++)
            {
                parameters.Add(fluent[i].Atom.Symbol);
            }
        }

        public PdSimFluentAssignment(PdSimAtom value, string fluentName, List<string> parameters)
        {
            this.value = value;
            this.fluentName = fluentName;
            this.parameters = parameters;
        }

        public override string ToString()
        {
            // Assignment in format: FluentName (Parameter1, Parameter2, ...) := Value
            string assignment = string.Format("{0} ", fluentName);
            if (parameters.Count > 0)
            {
                assignment += " (";
                foreach (var parameter in parameters)
                {
                    assignment += string.Format("{0}, ", parameter.ToString());
                }
                assignment = assignment.Remove(assignment.Length - 2);
                assignment += ")";
            }

            if (!value.IsEmpty())
                assignment += string.Format(" := {0}", value.ToString());

            return assignment;
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
        public string id;
        /// <summary>
        /// Action name.
        /// </summary>
        public string name;
        /// <summary>
        /// List of objects that are parameters of the action.
        /// </summary>
        public List<string> parameters;
        public float startTime;
        public float endTime;

        public PdSimActionInstance(ActionInstance actionInstance)
        {
            id = actionInstance.Id;

            name = actionInstance.ActionName;

            parameters = new List<string>();
            foreach (var parameter in actionInstance.Parameters)
            {
                parameters.Add(parameter.Symbol);
            }

            startTime = -1f;
            endTime = -1f;

            if (actionInstance.StartTime != null)
            {
                startTime = PdSimUtils.RealToFloat(actionInstance.StartTime);
                endTime = PdSimUtils.RealToFloat(actionInstance.EndTime);
            }
        }

        public override string ToString()
        {
            // Action instance in format: Name (Parameter1, Parameter2, ...)
            string actionInstance = string.Format("{0} (", name);
            foreach (var parameter in parameters)
            {
                actionInstance += string.Format("{0}, ", parameter.ToString());
            }
            actionInstance = actionInstance.Remove(actionInstance.Length - 2);
            actionInstance += ")";

            if (startTime >= 0)
                actionInstance += $" |{startTime}, {endTime}|";

            return actionInstance;
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
                    upperBound = PdSimUtils.RealToFloat(atomUpperBound.Real);
                }

                if (atomLowerBound.ContentCase == Atom.ContentOneofCase.Real)
                {
                    lowerBound = PdSimUtils.RealToFloat(atomLowerBound.Real);
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

    /// <summary>
    /// Utility class for PdSim.
    /// </summary>
    public static class PdSimUtils
    {
        // Convert Real class to float
        public static float RealToFloat(Real real)
        {
            return real.Numerator / real.Denominator;
        }

        // Convert unified planning domain value type to PdSim ValueType
        public static ValueType ConvertValueType(string type)
        {
            if (type == "up:integer")
                return ValueType.Int;
            else if (type == "up:real")
                return ValueType.Real;
            else if (type == "up:bool")
                return ValueType.Boolean;
            else
                return ValueType.Symbol;
        }
    }
}