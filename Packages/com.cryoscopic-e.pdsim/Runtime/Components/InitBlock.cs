using GeTPlan.Core.Models.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PDSim.Components
{
    /// <summary>
    /// Displays the initial state (init block) of the planning problem in the Unity Inspector.
    /// </summary>
    public class InitBlock : MonoBehaviour
    {
        #region Public API

        /// <summary>
        /// Singleton instance of the InitBlock component.
        /// </summary>
        public static InitBlock Instance
        {
            get
            {
                if (_instance == null)
                    _instance = FindAnyObjectByType<InitBlock>();
                return _instance;
            }
        }

        /// <summary>
        /// The list of state variables in the initial state.
        /// </summary>
        [Tooltip("The list of fluents and their values in the initial state.")]
        public List<InitBlockComponent> Components;

        /// <summary>
        /// Initializes the component with the initial state variables from the problem.
        /// </summary>
        /// <param name="stateVariables">The list of fluents and their values.</param>
        public void InitialiseComponent(List<(FluentExpression Fluent, object Value)> stateVariables)
        {
            Components = new List<InitBlockComponent>();
            foreach (var stateVariable in stateVariables)
            {
                var fluentName = stateVariable.Fluent.Name;
                var parameters = stateVariable.Fluent.Arguments
                    .Select(a => a is ConstantExpression c ? c.Value.ToString() : a.ToString())
                    .ToList();
                var value = stateVariable.Value?.ToString() ?? "null";

                Components.Add(new InitBlockComponent()
                {
                    FluentName = fluentName,
                    Parameters = parameters,
                    Value = value
                });
            }
        }

        #endregion

        #region Data Classes

        /// <summary>
        /// Represents a single fluent assignment in the initial state.
        /// </summary>
        [Serializable]
        public class InitBlockComponent
        {
            /// <summary>
            /// The name of the fluent.
            /// </summary>
            public string FluentName;
            /// <summary>
            /// The parameters of the fluent.
            /// </summary>
            public List<string> Parameters;
            /// <summary>
            /// The value assigned to the fluent.
            /// </summary>
            public string Value;

            /// <summary>
            /// Returns a string representation of the fluent assignment.
            /// </summary>
            /// <returns>A formatted string.</returns>
            public override string ToString()
            {
                return $"{FluentName}({string.Join(",", Parameters)}) := {Value}";
            }
        }

        #endregion

        #region Private Internals

        private static InitBlock _instance;

        #endregion
    }
}
