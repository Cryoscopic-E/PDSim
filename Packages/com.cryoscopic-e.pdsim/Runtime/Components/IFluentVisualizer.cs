using System;
using System.Collections.Generic;
using UnityEngine;

namespace PDSim.Components
{
    /// <summary>
    /// Interface for custom visualization logic for fluents.
    /// Implement this to provide fine-grained control over how a state change is animated.
    /// </summary>
    public interface IFluentVisualizer
    {
        /// <summary>
        /// Animates a state change for a fluent.
        /// </summary>
        /// <param name="args">List of fluent arguments (names).</param>
        /// <param name="value">The new value of the fluent.</param>
        /// <param name="objects">The GameObjects corresponding to the arguments.</param>
        /// <param name="duration">The duration of the action causing this state change.</param>
        /// <param name="onComplete">Callback to invoke when animation is finished.</param>
        void Animate(List<string> args, object value, GameObject[] objects, float duration, Action onComplete);
    }
}
