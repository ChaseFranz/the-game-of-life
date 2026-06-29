using System;

namespace TheGameOfLife.Core
{
    /// <summary>
    /// Raised by <see cref="IGameEngine.GenerationAdvanced"/> after the grid has
    /// been seeded or stepped. Carries the current generation number so listeners
    /// (e.g. the UI) can react without holding a reference back into the engine.
    /// </summary>
    public sealed class GenerationEventArgs : EventArgs
    {
        public GenerationEventArgs(int generation)
        {
            Generation = generation;
        }

        /// <summary>Generation index: 0 after <c>StartGame</c>, incremented per cycle.</summary>
        public int Generation { get; }
    }
}
