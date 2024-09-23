using System;
using System.Collections.Generic;

namespace PDSim.PlanningModel
{
        /// <summary>
        /// Class representing a planning domain fluent.
        /// e.g. (at ?x ?y)
        /// </summary>

        [Serializable]
        public class PdSimFluent
        {
            public string name;
            public PdSimAtom.ValueType type;
            public List<PdSimParameter> parameters;

            public PdSimFluent(Fluent fluent)
            {
                name = fluent.Name;
                type = PdSimAtom.ConvertValueType(fluent.ValueType);
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
    
}
