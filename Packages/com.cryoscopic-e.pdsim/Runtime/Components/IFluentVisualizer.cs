using System;
using System.Collections.Generic;
using UnityEngine;
using GeTPlan.Core.Models; using GeTPlan.Core.Logic; using GeTPlan.Core.Models.Expressions; using PDSimAPI;

namespace PDSim.Components
{
    public interface IFluentVisualizer
    {
        /// <summary>
        /// precise control over the animation.
        /// </summary>
        /// <param name="args">List of fluent arguments (names).</param>
        /// <param name="value">The new value of the fluent.</param>
        /// <param name="objects">The GameObjects corresponding to the arguments.</param>
        /// <param name="duration">The duration of the action causing this state change.</param>
        /// <param name="onComplete">Callback to invoke when animation is finished.</param>
        void Animate(List<string> args, object value, GameObject[] objects, float duration, Action onComplete);
    }
}
