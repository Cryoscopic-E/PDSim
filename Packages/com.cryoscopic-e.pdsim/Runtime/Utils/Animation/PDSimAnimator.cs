namespace PDSim.Utils.Animation
{
    /// <summary>
    /// Static factory class for creating animation builders.
    /// Provides entry points for building sequential and parallel animation sequences.
    /// </summary>
    public static class PDSimAnimator
    {
        #region Public API

        /// <summary>
        /// Creates a new animation builder for sequential animations (actions play one after another).
        /// </summary>
        /// <returns>A new <see cref="IAnimationBuilder"/> instance.</returns>
        public static IAnimationBuilder Sequence() => new AnimationBuilder(false);

        /// <summary>
        /// Creates a new animation builder for parallel animations (actions play simultaneously).
        /// </summary>
        /// <returns>A new <see cref="IAnimationBuilder"/> instance.</returns>
        public static IAnimationBuilder Parallel() => new AnimationBuilder(true);

        #endregion
    }
}
