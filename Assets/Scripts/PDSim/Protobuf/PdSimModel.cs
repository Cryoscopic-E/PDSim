using System;
using System.Collections.Generic;
using UnityEngine;

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

        public string _valueSymbol;

        public PdSimAtom(Atom atom)
        {
            contentCase = atom.ContentCase;
            switch (contentCase)
            {
                default:
                case Atom.ContentOneofCase.Symbol:
                    _valueSymbol = atom.Symbol;
                    break;
                case Atom.ContentOneofCase.Int:
                    _valueSymbol = atom.Int.ToString();
                    break;
                case Atom.ContentOneofCase.Real:
                    _valueSymbol = PdSimUtils.RealToFloat(atom.Real).ToString();
                    break;
                case Atom.ContentOneofCase.Boolean:
                    _valueSymbol = atom.Boolean.ToString();
                    break;
            }
        }

        public PdSimAtom()
        {
            contentCase = Atom.ContentOneofCase.Symbol;
            _valueSymbol = string.Empty;
        }

        public bool IsEmpty()
        {
            return _valueSymbol == string.Empty;
        }

        #region old
        //public object Value
        //{
        //    get
        //    {
        //        if (_contentCase == Atom.ContentOneofCase.Symbol)
        //            return (string)Value;
        //        else if (_contentCase == Atom.ContentOneofCase.Int)
        //            return (int)Value;
        //        else if (_contentCase == Atom.ContentOneofCase.Real)
        //            return (float)Value;
        //        else if (_contentCase == Atom.ContentOneofCase.Boolean)
        //            return (bool)Value;
        //        else
        //            return null;
        //    }
        //    private set { }
        //}

        //public PdSimAtom(Atom atom)
        //{
        //    _contentCase = atom.ContentCase;
        //    switch (_contentCase)
        //    {
        //        default:
        //        case Atom.ContentOneofCase.Symbol:
        //            Value = atom.Symbol;
        //            break;
        //        case Atom.ContentOneofCase.Int:
        //            Value = atom.Int;
        //            break;
        //        case Atom.ContentOneofCase.Real:
        //            Value = PdSimUtils.RealToFloat(atom.Real);
        //            break;
        //        case Atom.ContentOneofCase.Boolean:
        //            Value = atom.Boolean;
        //            break;
        //    }
        //}
        #endregion
        public override string ToString()
        {
            return _valueSymbol;
        }

        public static PdSimAtom Empty()
        {
            return new PdSimAtom(new Atom());
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


            effects = new List<PdSimEffect>();
            foreach (var effect in action.Effects)
            {
                effects.Add(new PdSimEffect(effect));
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
    public class PdSimEffect
    {

        public PdSimFluentAssignment fluentAssignment;
        public List<PdSimParameter> forAllVariables;
        public PdSimCondition effectCondition;
        public PdSimEffect(Effect effect)
        {
            var e = effect.Effect_;

            var value = new PdSimAtom(e.Value.Atom);

            var fluent = e.Fluent.List;
            var fluentName = fluent[0].Atom.Symbol;
            var parameters = new List<string>();
            for (int i = 1; i < fluent.Count; i++)
            {
                parameters.Add(fluent[i].Atom.Symbol);
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

            ////condition
            //if (e.Condition != null)
            //{
            //    var condition = e.Condition;
            //    if (condition.Kind == ExpressionKind.StateVariable)
            //    {
            //        stateVariableCondition = new PdSimStateVariableCondition(condition);
            //        functionApplicationCondition = null;
            //    }
            //    else if (condition.Kind == ExpressionKind.FunctionApplication)
            //    {
            //        functionApplicationCondition = new PdSimFunctionApplicationCondition(condition);
            //        stateVariableCondition = null;
            //    }
            //    else
            //    {
            //        stateVariableCondition = null;
            //        functionApplicationCondition = null;
            //    }
            //}
        }

        public bool IsForAll()
        {
            return forAllVariables.Count > 0;
        }

        public override string ToString()
        {
            string effect = "";
            if (forAllVariables.Count > 0)
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
            return effect;
        }
    }

    public enum EffectConditionType
    {
        None,
        Constant,
        StateVariable,
        FunctionApplication
    }

    /// <summary>
    /// Represents a condition in the action effect.
    /// </summary>
    [Serializable]
    public class PdSimCondition
    {
        public EffectConditionType type;
        public string symbol;
        public string functorSymbol;
        public List<PdSimFluentAssignment> fluents;

        public PdSimCondition(Expression expression)
        {
            symbol = string.Empty;
            functorSymbol = string.Empty;
            fluents = new List<PdSimFluentAssignment>();

            if (expression.Kind == ExpressionKind.StateVariable)
            {
                type = EffectConditionType.StateVariable;

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

                fluents.Add(new PdSimFluentAssignment(PdSimAtom.Empty(), fluentName, parameters));
            }
            else if (expression.Kind == ExpressionKind.FunctionApplication)
            {
                type = EffectConditionType.FunctionApplication;

                // first element is the functor symbol
                functorSymbol = expression.List[0].Atom.Symbol.Split(':')[1]; // form up:<functorSymbol>

                var fluentName = string.Empty;
                var parameters = new List<string>();
                var newFluent = true;
                for (var i = 1; i < expression.List.Count; i++)
                {
                    // parse list, every time a fluent symbol is found, create a new fluent
                    var exp = expression.List[i];
                    Debug.Log(exp);
                    if (exp.Kind == ExpressionKind.FluentSymbol)
                    {
                        if (!newFluent)
                        {
                            fluents.Add(new PdSimFluentAssignment(PdSimAtom.Empty(), fluentName, parameters));
                            parameters = new List<string>();
                        }
                        fluentName = exp.Atom.Symbol;
                        newFluent = true;
                    }
                    else if (exp.Kind == ExpressionKind.Variable || exp.Kind == ExpressionKind.Parameter)
                    {
                        parameters.Add(exp.Atom.Symbol);
                        newFluent = false;
                    }
                }

            }
            else if (expression.Kind == ExpressionKind.Constant)
            {
                type = EffectConditionType.Constant;
            }
            else
            {
                type = EffectConditionType.None;
            }

        }

        public override string ToString()
        {
            var condition = string.Empty;
            if (type == EffectConditionType.StateVariable)
            {
                condition += $"IF {fluents[0]} THEN\n";
            }
            else if (type == EffectConditionType.FunctionApplication)
            {
                condition += $"IF {functorSymbol} (";
                foreach (var fluent in fluents)
                {
                    condition += fluent.ToString();
                }
                condition = condition.Remove(condition.Length - 2);
                condition += ")\nTHEN\n";

            }
            else if (type == EffectConditionType.Constant)
            {
                condition = string.Empty;
            }
            else
            {
                condition = string.Empty;
            }
            return condition;
        }

    }

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