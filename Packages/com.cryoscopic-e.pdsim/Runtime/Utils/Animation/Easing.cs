using UnityEngine;

namespace PDSim.Utils.Animation
{
    /// <summary>
    /// Supported easing types for animations.
    /// </summary>
    public enum EasingType
    {
        Linear,
        InQuad,
        OutQuad,
        InOutQuad,
        InCubic,
        OutCubic,
        InOutCubic,
        InBack,
        OutBack,
        SmoothStep
    }

    /// <summary>
    /// Utility class for calculating easing functions.
    /// </summary>
    public static class Easing
    {
        /// <summary>
        /// Applies the specified easing function to a normalized time value.
        /// </summary>
        /// <param name="t">The normalized time (0 to 1).</param>
        /// <param name="type">The type of easing to apply.</param>
        /// <returns>The eased time value.</returns>
        public static float Apply(float t, EasingType type)
        {
            switch (type)
            {
                case EasingType.InQuad: return t * t;
                case EasingType.OutQuad: return t * (2 - t);
                case EasingType.InOutQuad: return t < 0.5f ? 2 * t * t : -1 + (4 - 2 * t) * t;
                case EasingType.InCubic: return t * t * t;
                case EasingType.OutCubic: return (--t) * t * t + 1;
                case EasingType.InOutCubic: return t < 0.5f ? 4 * t * t * t : (t - 1) * (2 * t - 2) * (2 * t - 2) + 1;
                case EasingType.InBack:
                    float s = 1.70158f;
                    return t * t * ((s + 1) * t - s);
                case EasingType.OutBack:
                    float s2 = 1.70158f;
                    return --t * t * ((s2 + 1) * t + s2) + 1;
                case EasingType.SmoothStep: return t * t * (3 - 2 * t);
                case EasingType.Linear:
                default: return t;
            }
        }
    }
}
