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
        public string Name { get; private set; }
        public string Type { get; private set; }

        public PdSimParameter(Parameter parameter)
        {
            Name = parameter.Name;
            Type = parameter.Type;
        }

        public override string ToString()
        {
            return string.Format("{0} - {1}", Name, Type);
        }
    }

    /// <summary>
    /// Class representing a planning domain fluent
    /// e.g. (at ?x ?y)
    /// </summary>

    [Serializable]
    public class PdSimFluent
    {
        public string Name { get; private set; }
        public ValueType Type { get; private set; }
        public List<PdSimParameter> Parameters { get; private set; }

        public PdSimFluent(Fluent fluent)
        {
            Name = fluent.Name;
            Type = PdSimUtils.ConvertValueType(fluent.ValueType);
            Parameters = new List<PdSimParameter>();
            foreach (var parameter in fluent.Parameters)
            {
                Parameters.Add(new PdSimParameter(parameter));
            }
        }

        public override string ToString()
        {
            // Fluent in format: Type::Name (Parameter1 - Type, Parameter2 - Type, ...)
            string fluent = string.Format("{0}::{1}", Type, Name);

            if (Parameters.Count > 0)
            {
                fluent += " (";
                foreach (var parameter in Parameters)
                {
                    fluent += string.Format("{0}, ", parameter.ToString());
                }
                fluent = fluent.Remove(fluent.Length - 2);
                fluent += ")";
            }
            return fluent;
        }
    }

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


}