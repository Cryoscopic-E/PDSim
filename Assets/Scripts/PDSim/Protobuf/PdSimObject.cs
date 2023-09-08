using System;

namespace PDSim.Protobuf
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

        public override string ToString()
        {
            return string.Format("{0} - {1}", Name, Type);
        }
    }
}