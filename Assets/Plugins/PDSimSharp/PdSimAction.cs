using System;

namespace PDSimSharp
{
    [Serializable]
    public class PdSimAction
    {
        private Action _action;

        public string Name { get; private set; }

        // duration

        // parameters

        // conditions

        // effects

        public PdSimAction(Action action)
        {
            Name = action.Name;

            var duration = action.Duration;

        }
    }
}