using System;
using System.Collections.Generic;
using UnityEngine;

namespace PDSim.Components
{
    /// <summary>
    /// Interface for custom visualization logic for actions.
    /// Implement this to provide fine-grained control over how an action is animated.
    /// </summary>
    public interface IActionVisualizer
    {
        /// <summary>
        /// Animates a grounded action.
        /// </summary>
        /// <param name="args">Ordered list of grounded argument names.</param>
        /// <param name="objects">The GameObjects corresponding to the arguments, in the same order.</param>
        /// <param name="duration">The duration of the action.</param>
        /// <param name="onComplete">Callback to invoke when animation is finished.</param>
        void Animate(List<string> args, GameObject[] objects, float duration, Action onComplete);
    }
}
