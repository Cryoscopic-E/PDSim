using System;
using System.Collections.Generic;
using UnityEngine;

namespace PDSim.Protobuf
{
    /// <summary>
    /// Class representing an atom.
    /// e.g 3, true, kitchen, at-robot
    /// </summary>
    public class PdSimAtom
    {
        private Atom.ContentOneofCase _contentCase;
        public object Value
        {
            get
            {
                if (_contentCase == Atom.ContentOneofCase.Symbol)
                    return (string)Value;
                else if (_contentCase == Atom.ContentOneofCase.Int)
                    return (int)Value;
                else if (_contentCase == Atom.ContentOneofCase.Real)
                    return (float)Value;
                else if (_contentCase == Atom.ContentOneofCase.Boolean)
                    return (bool)Value;
                else
                    return null;
            }
            private set { }
        }

        public PdSimAtom(Atom atom)
        {
            _contentCase = atom.ContentCase;
            switch (_contentCase)
            {
                default:
                case Atom.ContentOneofCase.Symbol:
                    Value = atom.Symbol;
                    break;
                case Atom.ContentOneofCase.Int:
                    Value = atom.Int;
                    break;
                case Atom.ContentOneofCase.Real:
                    Value = PdSimUtils.RealToFloat(atom.Real);
                    break;
                case Atom.ContentOneofCase.Boolean:
                    Value = atom.Boolean;
                    break;
            }
        }
        public override string ToString()
        {
            return Value.ToString();
        }
    }

    public class PdSimExpression
    {
        PdSimAtom atom;

        ExpressionKind kind;

        List<PdSimExpression> expressions;

        public PdSimExpression(Expression expression)
        {
            atom = expression.Atom != null ? new PdSimAtom(expression.Atom) : null;
            kind = expression.Kind;
        }

        public override string ToString()
        {
            if (atom != null)
                return atom.ToString();
            else if (expressions != null)
            {
                string expression = "";
                foreach (var exp in expressions)
                {
                    expression += exp.ToString();
                }
                return expression;
            }
            else
                return "";
        }

    }



    /// <summary>
    /// Class representing a parameter for a fluent, action or function
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

        public override string ToString()
        {
            return string.Format("{0} - {1}", name, type);
        }
    }

    /// <summary>
    /// Class representing a planning domain fluent
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


    [Serializable]
    public enum ValueType
    {
        Symbol,
        Int,
        Real,
        Boolean
    }


    public static class PdSimUtils
    {
        public static float RealToFloat(Real real)
        {
            return real.Numerator / real.Denominator;
        }

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


    /// <summary>
    /// Base class that represents an action in the domain.
    /// </summary>
    public abstract class PdSimAction
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

        /// <summary>
        /// List of preconditions of the action.
        /// </summary>
        public List<PdSimCondition> preconditions;
        /// <summary>
        /// List of effects of the action.
        /// </summary>
        public List<PdSimEffect> effects;
        public PdSimAction(Action action)
        {
            name = action.Name;

            parameters = new List<PdSimParameter>();
            foreach (var parameter in action.Parameters)
            {
                parameters.Add(new PdSimParameter(parameter));
            }

            preconditions = new List<PdSimCondition>();

            effects = new List<PdSimEffect>();

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
        public PdSimDuration duration;
        public PdSimDurativeAction(Action action) : base(action)
        {
            duration = new PdSimDuration(action.Duration);
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
    }

    [Serializable]
    public class PdSimDuration
    {
        public float upperBound;
        public float lowerBound;

        public PdSimDuration(Duration duration)
        {
            var atomUpperBound = duration.ControllableInBounds.Upper.Atom;
            var atomLowerBound = duration.ControllableInBounds.Lower.Atom;

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

}