using System.Collections.Generic;
using System.Linq;

namespace PDSim.Utils
{
    public static class IterUtils
    {
        public static IEnumerable<IEnumerable<T>> CartesianProduct<T>(this IEnumerable<IEnumerable<T>> sequences)
        {
            IEnumerable<IEnumerable<T>> emptyProduct = new[] { Enumerable.Empty<T>() };
            return sequences.Aggregate(
                emptyProduct,
                (accumulator, sequence) =>
                    from acc in accumulator
                    from item in sequence
                    select acc.Concat(new[] { item }));
        }
    }
}