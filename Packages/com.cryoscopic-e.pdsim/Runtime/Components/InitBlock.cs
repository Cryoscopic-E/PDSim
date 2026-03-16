using GeTModel;
using System;
using System.Collections.Generic;
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


        public void InitialiseComponent(List<GeTStateVariable> stateVariables)
        {
            Components = new List<InitBlockComponent>();
            foreach (var stateVariable in stateVariables)
            {
                var fluentName = stateVariable.Fluent.FluentName;
                var parameters = stateVariable.GetParameters();
                var value = stateVariable.Value.Atom.ToString();

                if (value != null)
                {
                    Components.Add(new InitBlockComponent()
                    {
                        FluentName = fluentName,
                        Parameters = parameters,
                        Value = value
                    });
                }
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
