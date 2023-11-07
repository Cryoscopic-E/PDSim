using PDSim.Protobuf;
using System.Collections.Generic;

namespace PDSim.Utils
{
    /// <summary>
    /// Helper class to generate unique animation names
    /// </summary>
    public static class AnimationNames
    {
        // public static string UniqueBooleanAnimationName(PdSimAtom value, string predicateName, List<string> attributeTypes)
        // {
        //     var animationName = predicateName;
        //     if (!value.IsTrue())
        //         animationName = "NOT_" + animationName;

        //     foreach (var item in attributeTypes)
        //     {
        //         animationName += "_" + item;
        //     }

        //     return animationName;
        // }

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