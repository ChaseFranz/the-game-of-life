using System;
using System.Threading.Tasks;

namespace TheGameOfLife.Core
{
    /// <summary>
    /// Conway's Game of Life simulation. UI-independent: it seeds and advances a
    /// grid of <see cref="Cell"/> and notifies observers via
    /// <see cref="GenerationAdvanced"/>. The grid does not wrap — cells off the
    /// edge are treated as dead.
    /// </summary>
    public sealed class GameEngine : IGameEngine
    {
        private readonly GameConfig _config;
        private readonly Random _random;

        public Cell[,] Cells { get; private set; }

        public int Rows => _config.Rows;
        public int Columns => _config.Columns;
        public int Generation { get; private set; }

        public event EventHandler<GenerationEventArgs>? GenerationAdvanced;

        public GameEngine(GameConfig config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));

            if (config.Rows <= 0)
                throw new ArgumentOutOfRangeException(nameof(config), "Rows must be positive.");
            if (config.Columns <= 0)
                throw new ArgumentOutOfRangeException(nameof(config), "Columns must be positive.");
            if (config.InitialLiveProbability < 0.0 || config.InitialLiveProbability > 1.0)
                throw new ArgumentOutOfRangeException(nameof(config), "InitialLiveProbability must be in [0, 1].");

            _random = config.RandomSeed is int seed ? new Random(seed) : new Random();
            Cells = new Cell[0, 0];
        }

        public void StartGame()
        {
            GenerateCells();
            Generation = 0;
            OnGenerationAdvanced();
        }

        public void NextCycle()
        {
            UpdateGridState();
            Generation++;
            OnGenerationAdvanced();
        }

        /// <summary>Counts the living cells among the eight Moore neighbours of (row, column).</summary>
        public int GetLivingNeighborCount(int row, int column)
        {
            int liveCount = 0;
            int rowCount = Cells.GetLength(0);
            int columnCount = Cells.GetLength(1);

            bool notFirstRow = row - 1 >= 0;
            bool notFirstColumn = column - 1 >= 0;
            bool notLastColumn = column + 1 < columnCount; // 0-based index
            bool notLastRow = row + 1 < rowCount;          // 0-based index

            // Top-Middle
            liveCount += (notFirstRow && Cells[row - 1, column].Alive) ? 1 : 0;

            // Top-Left
            liveCount += ((notFirstColumn && notFirstRow) && Cells[row - 1, column - 1].Alive) ? 1 : 0;

            // Top-Right
            liveCount += ((notLastColumn && notFirstRow) && Cells[row - 1, column + 1].Alive) ? 1 : 0;

            // Mid-Left
            liveCount += (notFirstColumn && Cells[row, column - 1].Alive) ? 1 : 0;

            // Mid-Right
            liveCount += (notLastColumn && Cells[row, column + 1].Alive) ? 1 : 0;

            // Bottom-Left
            liveCount += ((notFirstColumn && notLastRow) && Cells[row + 1, column - 1].Alive) ? 1 : 0;

            // Bottom-Right
            liveCount += ((notLastColumn && notLastRow) && Cells[row + 1, column + 1].Alive) ? 1 : 0;

            // Bottom-Mid
            liveCount += (notLastRow && Cells[row + 1, column].Alive) ? 1 : 0;

            return liveCount;
        }

        /// <summary>Allocates the grid and seeds each cell alive with the configured probability.</summary>
        private void GenerateCells()
        {
            Cells = new Cell[_config.Rows, _config.Columns];
            int rowCount = Cells.GetLength(0);
            int columnCount = Cells.GetLength(1);

            for (int row = 0; row < rowCount; row++)
            {
                for (int column = 0; column < columnCount; column++)
                {
                    Cells[row, column] = new Cell
                    {
                        Alive = _random.NextDouble() < _config.InitialLiveProbability
                    };
                }
            }
        }

        /// <summary>
        /// Applies the B3/S23 rule in two passes so the update is consistent: the
        /// first pass computes every cell's <see cref="Cell.NextAlive"/> from the
        /// current grid, the second commits it. Both passes parallelise over rows.
        /// </summary>
        private void UpdateGridState()
        {
            int rows = Cells.GetLength(0);
            int columns = Cells.GetLength(1);

            Parallel.For(0, rows, rowIndex =>
            {
                for (int columnIndex = 0; columnIndex < columns; columnIndex++)
                {
                    var currentCell = Cells[rowIndex, columnIndex];
                    int liveNeighborCount = GetLivingNeighborCount(rowIndex, columnIndex);

                    currentCell.NextAlive = currentCell.Alive
                        ? liveNeighborCount == 2 || liveNeighborCount == 3 // survival
                        : liveNeighborCount == 3;                          // birth
                }
            });

            Parallel.For(0, rows, rowIndex =>
            {
                for (int columnIndex = 0; columnIndex < columns; columnIndex++)
                {
                    Cells[rowIndex, columnIndex].Alive = Cells[rowIndex, columnIndex].NextAlive;
                }
            });
        }

        private void OnGenerationAdvanced()
        {
            GenerationAdvanced?.Invoke(this, new GenerationEventArgs(Generation));
        }
    }
}
