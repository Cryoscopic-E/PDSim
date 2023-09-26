using PDSim.Protobuf;
using System.Collections.Generic;

namespace PDSim.Utils
{
    public static class AnimationNames
    {
        public static string UniqueAnimationName(PdSimAtom value, string predicateName, List<string> attributeTypes)
        {
            var animationName = predicateName;
            if (!value.IsTrue())
                animationName = "NOT_" + animationName;

            foreach (var item in attributeTypes)
            {
                animationName += "_" + item;
            }

            return animationName;
        }

        public static string UniqueAnimationName(PdSimFluentAssignment predicate)
        {
            return UniqueAnimationName(predicate.value, predicate.fluentName, predicate.parameters);
        }
    }
}