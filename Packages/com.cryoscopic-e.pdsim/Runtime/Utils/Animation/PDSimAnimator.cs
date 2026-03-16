using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PDSim.Utils.Animation
{
    public static class PDSimAnimator
    {
        public static IAnimationBuilder Sequence() => new AnimationBuilder(false);
        public static IAnimationBuilder Parallel() => new AnimationBuilder(true);
    }
}
