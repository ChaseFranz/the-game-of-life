namespace TheGameOfLife.Core
{
    /// <summary>
    /// Configuration for a <see cref="GameEngine"/> run. Grid resolution is
    /// expressed independently of any rendering surface, so the number of cells
    /// the simulation tracks is no longer tied to the window's pixel size.
    /// </summary>
    public sealed class GameConfig
    {
        /// <summary>Number of cell rows in the grid. Must be positive.</summary>
        public int Rows { get; init; }

        /// <summary>Number of cell columns in the grid. Must be positive.</summary>
        public int Columns { get; init; }

        /// <summary>
        /// Probability (0.0–1.0) that a given cell starts alive when the grid is
        /// seeded. Defaults to the original 1-in-10 density.
        /// </summary>
        public double InitialLiveProbability { get; init; } = 0.1;

        /// <summary>
        /// Optional fixed seed for the random number generator. When set, seeding
        /// is deterministic (useful for tests and reproducible runs); when null a
        /// time-based seed is used.
        /// </summary>
        public int? RandomSeed { get; init; }
    }
}
