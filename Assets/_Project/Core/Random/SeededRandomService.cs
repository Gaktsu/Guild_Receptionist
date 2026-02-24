using System;

namespace Project.Core.Random
{
    /// <summary>
    /// Deterministic random service backed by <see cref="System.Random"/>.
    /// </summary>
    public sealed class SeededRandomService : IRandomService
    {
        private readonly System.Random _random;

        /// <summary>
        /// Creates a deterministic random service with an externally provided seed.
        /// </summary>
        public SeededRandomService(int seed)
        {
            _random = new System.Random(seed);
        }

        /// <inheritdoc />
        public int Range(int minInclusive, int maxExclusive)
        {
            if (maxExclusive <= minInclusive)
            {
                throw new ArgumentException(
                    $"{nameof(maxExclusive)} must be greater than {nameof(minInclusive)}.",
                    nameof(maxExclusive));
            }

            return _random.Next(minInclusive, maxExclusive);
        }

        /// <inheritdoc />
        public float Value()
        {
            return (float)_random.NextDouble();
        }
    }
}
