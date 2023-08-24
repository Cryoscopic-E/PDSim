using System;

namespace PDSimSharp
{

    [Serializable]
    public class Parameter
    {
        public string Name { get; private set; }
        public string Type { get; private set; }

        public Parameter(string name, string type)
        {
            Name = name;
            Type = type;
        }
    }
}