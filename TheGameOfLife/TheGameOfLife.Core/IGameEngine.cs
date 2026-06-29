using System;

namespace TheGameOfLife.Core
{
    /// <summary>
    /// The simulation core. Owns the cell grid and advances it according to
    /// Conway's rules. It has no dependency on any UI; consumers observe progress
    /// through <see cref="GenerationAdvanced"/> rather than being called back
    /// through a mutual reference.
    /// </summary>
    public interface IGameEngine
    {
        /// <summary>The current grid. Read-only reference; cells mutate in place.</summary>
        Cell[,] Cells { get; }

        /// <summary>Number of rows in the grid.</summary>
        int Rows { get; }

        /// <summary>Number of columns in the grid.</summary>
        int Columns { get; }

        /// <summary>The current generation number (0 immediately after seeding).</summary>
        int Generation { get; }

        /// <summary>Raised after the grid is seeded or advanced one generation.</summary>
        event EventHandler<GenerationEventArgs>? GenerationAdvanced;

        /// <summary>Seeds a fresh grid and raises <see cref="GenerationAdvanced"/>.</summary>
        void StartGame();

        /// <summary>Advances the grid one generation and raises <see cref="GenerationAdvanced"/>.</summary>
        void NextCycle();
    }
}
