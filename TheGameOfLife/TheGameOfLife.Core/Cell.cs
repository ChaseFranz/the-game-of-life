namespace TheGameOfLife.Core
{
    /// <summary>
    /// A single grid cell. Holds only state — the live/dead seeding policy and
    /// the transition rules live in <see cref="GameEngine"/>.
    /// </summary>
    public sealed class Cell
    {
        /// <summary>Whether the cell is currently alive.</summary>
        public bool Alive { get; set; }

        /// <summary>
        /// The cell's living state for the next generation. Computed in the first
        /// pass of an update and applied in the second, so neighbour reads stay
        /// consistent while the grid is updated in place.
        /// </summary>
        public bool NextAlive { get; set; }
    }
}
