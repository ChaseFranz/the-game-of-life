﻿using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace TheGameOfLife
{
    public class GameEngine : IGameEngine
    {
        public  Cell[,] Cells { get; set; }

        public int GridHeight { get; set; }
        public int GridWidth { get; set; }

        public IClient GameClient { get; set; }

        public GameEngine()
        {
        }

        /// <summary>
        /// This method is responsible for generating a collection of cells before each game starts
        /// </summary>
        private void GenerateCells()
        {
            Cells = new Cell[GridHeight, GridWidth];

            int rowCount = Cells.GetLength(0);
            int columnCount = Cells.GetLength(1);

            // Enumerate row-wise
            for (int row = 0; row < rowCount; row++)
            {
                for (int column = 0; column < columnCount; column++)
                {
                    // Create cell and add to collections
                    Cell cell = new Cell();
                    Cells[row, column] = cell;
                }
            }
        }

        public void StartGame()
        {
            GenerateCells();
            GameClient.RefreshClient();
        }

        public void NextCycle()
        {
            UpdateGridState();
            GameClient.RefreshClient();
        }

        public int GetLivingNeighborCount(int row, int column)
        {
            int liveCount = 0;
            int rowCount = Cells.GetLength(0);
            int columnCount = Cells.GetLength(1);

            bool notFirstRow = row - 1 >= 0;
            bool notFirstColumn = column - 1 >= 0;
            bool notLastColumn = column + 1 < columnCount; // 0-based index
            bool notLastRow = row + 1 < rowCount; // 0-based index
            Cell neighborCell;

            // Top-Middle
            if (notFirstRow)
            {
                neighborCell = Cells[row - 1, column];
                liveCount += neighborCell.Alive ? 1 : 0;
            }

            // Top-Left
            if (notFirstColumn && notFirstRow)
            {
                neighborCell = Cells[row - 1, column - 1];
                liveCount += neighborCell.Alive ? 1 : 0;
            }

            // Top-Right
            if (notLastColumn && notFirstRow)
            {
                neighborCell = Cells[row - 1, column + 1];
                liveCount += neighborCell.Alive ? 1 : 0;
            }

            // Mid-Left
            if (notFirstColumn)
            {
                neighborCell = Cells[row, column - 1];
                liveCount += neighborCell.Alive ? 1 : 0;
            }

            // Mid-Right
            if (notLastColumn)
            {
                neighborCell = Cells[row, column + 1];
                liveCount += neighborCell.Alive ? 1 : 0;
            }

            // Bottom-Left
            if (notFirstColumn && notLastRow)
            {
                neighborCell = Cells[row + 1, column - 1];
                liveCount += neighborCell.Alive ? 1 : 0;
            }

            // Bottom-Right
            if (notLastColumn && notLastRow)
            {
                neighborCell = Cells[row + 1, column + 1];
                liveCount += neighborCell.Alive ? 1 : 0;
            }

            // Bottom-Mid
            if (notLastRow)
            {
                neighborCell = Cells[row + 1, column];
                liveCount += neighborCell.Alive ? 1 : 0;
            }
            return liveCount;
        }

        private void UpdateGridState()
        {
            ConcurrentBag<Cell> deadCells = new ConcurrentBag<Cell>();
            ConcurrentBag<Cell> liveCells = new ConcurrentBag<Cell>();

            int rows = Cells.GetLength(0);
            int columns = Cells.GetLength(1);

            Parallel.For(0, rows, rowIndex =>
            {
                for (int columnIndex = 0; columnIndex < columns; columnIndex++)
                {
                    var currentCell = Cells[rowIndex, columnIndex];
                    int liveNeighborCount = GetLivingNeighborCount(rowIndex, columnIndex);
                    bool isLiving = currentCell.Alive;

                    if (isLiving || liveNeighborCount > 1)
                    {
                        if ((!isLiving && liveNeighborCount == 3) || (liveNeighborCount == 2 || liveNeighborCount == 3))
                        {
                            liveCells.Add(currentCell);
                        }
                        else
                        {
                            deadCells.Add(currentCell);
                        }
                    }
                }
            });

            Parallel.ForEach(deadCells, currentCell =>
            {
                currentCell.Alive = false;
            });

            Parallel.ForEach(liveCells, currentCell =>
            {
                currentCell.Alive = true;
            });
        }
    }
}
