using System.Collections.Generic;

namespace PDSim.Utils
{
    /// <summary>
    /// Helper class to generate unique animation names
    /// </summary>
    public static class AnimationNames
    {
        public static string UniqueAnimationName(string predicateName, List<string> attributeTypes)
        {
            var animationName = predicateName;

            foreach (var item in attributeTypes)
            {
                animationName += "_" + item;
            }

            return animationName;
        }
    }
}