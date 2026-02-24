namespace Project.Core.Random
{
    /// <summary>
    /// Seeded random number service for deterministic gameplay logic.
    /// </summary>
    public interface IRandomService
    {
        /// <summary>
        /// Returns a random integer in [minInclusive, maxExclusive).
        /// </summary>
        int Range(int minInclusive, int maxExclusive);

        /// <summary>
        /// Returns a random float in [0.0f, 1.0f).
        /// </summary>
        float Value();
    }
}
