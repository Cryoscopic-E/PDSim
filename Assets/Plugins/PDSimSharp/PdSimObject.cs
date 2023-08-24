using System;

namespace PDSimSharp
{
    [Serializable]
    public class PdSimObject
    {
        public string Name { get; private set; }
        public string Type { get; private set; }

        public PdSimObject(string name, string type)
        {
            Name = name;
            Type = type;
        }
    }
}