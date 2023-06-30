using PDSim.Components;
using System.Collections.Generic;

namespace PDSim.Utils
{
    public static class AnimationNames
    {
        public static string UniqueAnimationName(bool negated, string predicateName, List<string> attributeTypes)
        {
            var animationName = predicateName;
            if (negated)
                animationName = "NOT_" + animationName;

            foreach (var item in attributeTypes)
            {
                animationName += "_" + item;
            }

            return animationName;
        }

        public static string UniqueAnimationName(PdBooleanPredicate predicate)
        {
            return UniqueAnimationName(predicate.value, predicate.name, predicate.attributes);
        }
    }
}