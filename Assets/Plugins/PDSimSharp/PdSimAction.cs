using System;

namespace PDSimSharp
{
    [Serializable]
    public class PdSimAction
    {
        public string Name { get; private set; }

        // duration

        // parameters

        // conditions

        // effects

        public PdSimAction(Action action)
        {
            Name = action.Name;
        }
    }
}