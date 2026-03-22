using GeTPlan.Core.Models;
using GeTPlan.Core.Logic;
using GeTPlan.Core.Models.Expressions;
using PDSimAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PDSim.Components
{
    public class InitBlock : MonoBehaviour
    {
        // Singleton Instance
        // ------------------

        private static InitBlock _instance;
        public static InitBlock Instance
        {
            get
            {
                if (_instance == null)
                    _instance = FindAnyObjectByType<InitBlock>();
                return _instance;
            }
        }

        public List<InitBlockComponent> Components;


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

        [Serializable]
        public class InitBlockComponent
        {
            public string FluentName;
            public List<string> Parameters;
            public string Value;

            public override string ToString()
            {
                return $"{FluentName}({string.Join(",", Parameters)}) := {Value}";
            }
        }
    }
}
