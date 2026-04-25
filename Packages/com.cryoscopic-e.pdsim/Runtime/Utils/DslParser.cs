using System;
using System.Collections.Generic;
using System.Linq;
using GeTPlan.Core.Models.Expressions;

namespace PDSim.Runtime.Utils
{
    /// <summary>
    /// A simple parser to convert string DSL expressions into PlanExpressions.
    /// Supports: fluent[arg1, arg2], !fluent[args], and basic literals.
    /// </summary>
    public static class DslParser
    {
        #region Public API

        /// <summary>
        /// Parses a string expression into a <see cref="PlanExpression"/>.
        /// </summary>
        /// <param name="expression">The string expression to parse.</param>
        /// <param name="selfName">The name to substitute for 'self'.</param>
        /// <param name="hitName">The name to substitute for 'hit'.</param>
        /// <returns>The parsed <see cref="PlanExpression"/>, or null if the expression is empty.</returns>
        public static PlanExpression Parse(string expression, string selfName = "", string hitName = "")
        {
            expression = expression.Trim();
            if (string.IsNullOrEmpty(expression))
            {
                return null;
            }

            // Handle NOT
            if (expression.StartsWith("!"))
            {
                var inner = Parse(expression.Substring(1), selfName, hitName);
                return !inner;
            }

            // Handle assignments (fluent := value)
            if (expression.Contains(":="))
            {
                var parts = expression.Split(new[] { ":=" }, StringSplitOptions.RemoveEmptyEntries);
                var fluent = ParseFluent(parts[0].Trim(), selfName, hitName);
                var value = Parse(parts[1].Trim(), selfName, hitName);
                return new AssignmentExpression(fluent, value);
            }

            // Handle boolean literals
            if (bool.TryParse(expression, out bool b))
            {
                return new ConstantExpression(b);
            }

            // Handle numeric literals
            if (double.TryParse(expression, out double d))
            {
                return new ConstantExpression(d);
            }

            // Handle strings (potentially objects or fluents)
            if (expression.Contains("["))
            {
                return ParseFluent(expression, selfName, hitName);
            }

            // Treat as parameter or constant
            if (expression == "self")
            {
                return new ParameterExpression(selfName);
            }
            if (expression == "hit")
            {
                return new ParameterExpression(hitName);
            }

            return new ParameterExpression(expression);
        }

        #endregion

        #region Private Internals

        /// <summary>
        /// Parses a fluent expression string into a <see cref="FluentExpression"/>.
        /// </summary>
        /// <param name="expression">The fluent expression string (e.g., "fluent[arg1, arg2]").</param>
        /// <param name="selfName">The name to substitute for 'self'.</param>
        /// <param name="hitName">The name to substitute for 'hit'.</param>
        /// <returns>The parsed <see cref="FluentExpression"/>.</returns>
        private static FluentExpression ParseFluent(string expression, string selfName, string hitName)
        {
            int openBracket = expression.IndexOf('[');
            int closeBracket = expression.LastIndexOf(']');

            if (openBracket != -1 && closeBracket != -1 && closeBracket > openBracket)
            {
                string fluentName = expression.Substring(0, openBracket).Trim();
                string argsString = expression.Substring(openBracket + 1, closeBracket - openBracket - 1);
                string[] args = string.IsNullOrEmpty(argsString) ? new string[0] : argsString.Split(',').Select(a => a.Trim()).ToArray();

                var planArgs = new List<PlanExpression>();
                foreach (var arg in args)
                {
                    if (arg == "self")
                    {
                        planArgs.Add(new ConstantExpression(selfName));
                    }
                    else if (arg == "hit")
                    {
                        planArgs.Add(new ConstantExpression(hitName));
                    }
                    else
                    {
                        planArgs.Add(new ConstantExpression(arg));
                    }
                }
                return new FluentExpression(fluentName, planArgs.AsReadOnly());
            }

            return new FluentExpression(expression);
        }

        #endregion
    }
}
