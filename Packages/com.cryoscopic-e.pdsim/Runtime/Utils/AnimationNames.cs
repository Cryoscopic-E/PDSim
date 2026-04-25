using System.Collections.Generic;

namespace PDSim.Utils
{
    /// <summary>
    /// Utility class for generating unique animation names based on predicates and attribute types.
    /// </summary>
    public static class AnimationNames
    {
        #region Public API

        /// <summary>
        /// Generates a unique animation name by combining a predicate name with its attribute types.
        /// </summary>
        /// <param name="predicateName">The name of the predicate.</param>
        /// <param name="attributeTypes">A list of attribute type names.</param>
        /// <returns>A formatted string representing the unique animation name.</returns>
        public static string UniqueAnimationName(string predicateName, List<string> attributeTypes)
        {
            var animationName = predicateName;

            foreach (var item in attributeTypes)
            {
                animationName += "_" + item;
            }

            return animationName;
        }

        #endregion
    }
}
