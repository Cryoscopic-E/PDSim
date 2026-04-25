using System;
using System.Collections.Generic;
using UnityEngine;
using GeTPlan.Core.Models;
using PDSim.Runtime.Utils;

namespace PDSim.Components.Interactive
{
    /// <summary>
    /// LogicalAction allows defining planning actions directly on GameObjects in the Inspector.
    /// Bridges the gap between Unity GameObjects and PDDL-style action definitions.
    /// </summary>
    public class LogicalAction : MonoBehaviour
    {
        #region Data Classes

        /// <summary>
        /// Represents a parameter for the logical action.
        /// </summary>
        [Serializable]
        public struct Parameter
        {
            /// <summary>
            /// The name of the parameter.
            /// </summary>
            public string Name;
            /// <summary>
            /// The type of the parameter in the planning domain.
            /// </summary>
            public string Type;
        }

        #endregion

        #region Public API

        /// <summary>
        /// The unique name of the action.
        /// </summary>
        [Header("Action Metadata")]
        [Tooltip("The unique name of the action.")]
        public string ActionName;

        /// <summary>
        /// The list of parameters for this action.
        /// </summary>
        [Tooltip("The list of parameters for this action.")]
        public List<Parameter> Parameters = new List<Parameter>();

        /// <summary>
        /// The list of preconditions in DSL format.
        /// </summary>
        [Header("Logic (DSL)")]
        [Tooltip("The list of preconditions in DSL format.")]
        public List<string> Preconditions = new List<string>();

        /// <summary>
        /// The list of effects in DSL format.
        /// </summary>
        [Tooltip("The list of effects in DSL format.")]
        public List<string> Effects = new List<string>();

        /// <summary>
        /// Whether the action is currently executable based on the world state.
        /// For Inspector feedback.
        /// </summary>
        public bool IsExecutable { get; private set; }

        /// <summary>
        /// Checks if the action is executable given specific object bindings and the current live state.
        /// </summary>
        /// <param name="bindings">The object bindings for the action parameters.</param>
        /// <returns>True if all preconditions are met.</returns>
        public bool CheckPreconditions(IReadOnlyDictionary<string, PlanObject> bindings)
        {
            if (PDSimWorldObserver.Instance == null) return false;

            var state = PDSimWorldObserver.Instance.LiveState;

            // Add 'self' to bindings if not present
            var extendedBindings = new Dictionary<string, PlanObject>(bindings);
            if (!extendedBindings.ContainsKey("self"))
            {
                // Find our own PlanObject from registry or create a temporary one
                extendedBindings["self"] = new PlanObject(gameObject.name, new PlanType("object"));
            }

            foreach (var preStr in Preconditions)
            {
                var expr = DslParser.Parse(preStr, gameObject.name);
                if (expr != null)
                {
                    var result = GeTPlan.Core.Logic.ExpressionEvaluator.Evaluate(expr, state, extendedBindings);
                    if (result is bool b && !b) return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Converts this component definition into a GeTPlan ActionDefinition.
        /// </summary>
        /// <param name="domainTypes">The types available in the planning domain.</param>
        /// <returns>A new ActionDefinition instance.</returns>
        public ActionDefinition ToActionDefinition(IReadOnlyDictionary<string, PlanType> domainTypes)
        {
            var action = new ActionDefinition(string.IsNullOrEmpty(ActionName) ? gameObject.name : ActionName);

            foreach (var p in Parameters)
            {
                var type = domainTypes.TryGetValue(p.Type, out var t) ? t : new PlanType(p.Type);
                action.WithParam(p.Name, type);
            }

            foreach (var pre in Preconditions)
            {
                var expr = DslParser.Parse(pre, "self"); // Use placeholder 'self' for lifted definition
                if (expr != null) action.Precondition(expr);
            }

            foreach (var eff in Effects)
            {
                var expr = DslParser.Parse(eff, "self");
                if (expr != null) action.Effect(expr);
            }

            return action;
        }

        #endregion

        #region Unity Lifecycle

        private void OnEnable()
        {
            if (PDSimWorldObserver.Instance != null)
            {
                PDSimWorldObserver.Instance.RegisterAction(this);
            }
        }

        private void OnDisable()
        {
            if (PDSimWorldObserver.Instance != null)
            {
                PDSimWorldObserver.Instance.UnregisterAction(this);
            }
        }

        #endregion
    }
}
