using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using GeTPlan.Core.Models;
using GeTPlan.Core.Logic;
using GeTPlan.Core.Models.Expressions;
using PDSim.Runtime.Utils;

namespace PDSim.Runtime.Components
{
    /// <summary>
    /// LogicalAction allows defining planning actions directly on GameObjects in the Inspector.
    /// </summary>
    public class LogicalAction : MonoBehaviour
    {
        [Serializable]
        public struct Parameter
        {
            public string name;
            public string type;
        }

        [Header("Action Metadata")]
        public string actionName;
        public List<Parameter> parameters = new List<Parameter>();

        [Header("Logic (DSL)")]
        public List<string> preconditions = new List<string>();
        public List<string> effects = new List<string>();

        // For Inspector feedback
        public bool IsExecutable { get; private set; }

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

        /// <summary>
        /// Checks if the action is executable given specific object bindings.
        /// </summary>
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

            foreach (var preStr in preconditions)
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
        public ActionDefinition ToActionDefinition(IReadOnlyDictionary<string, PlanType> domainTypes)
        {
            var action = new ActionDefinition(string.IsNullOrEmpty(actionName) ? gameObject.name : actionName);
            
            foreach (var p in parameters)
            {
                var type = domainTypes.TryGetValue(p.type, out var t) ? t : new PlanType(p.type);
                action.WithParam(p.name, type);
            }

            foreach (var pre in preconditions)
            {
                var expr = DslParser.Parse(pre, "self"); // Use placeholder 'self' for lifted definition
                if (expr != null) action.Precondition(expr);
            }

            foreach (var eff in effects)
            {
                var expr = DslParser.Parse(eff, "self");
                if (expr != null) action.Effect(expr);
            }

            return action;
        }
    }
}
