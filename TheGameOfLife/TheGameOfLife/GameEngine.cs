using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace TheGameOfLife
{
    public class GameEngine : IGameEngine
    {
        public  Cell[,] Cells { get; set; }

        public ConcurrentBag<Cell> CellBag { get; set; }
        public int GridHeight { get; set; }
        public int GridWidth { get; set; }

        public IClient GameClient { get; set; }

        public GameEngine()
        {
            CellBag = new ConcurrentBag<Cell>();
        }

        /// <summary>
        /// This method is responsible for generating a collection of cells before each game starts
        /// </summary>
        private void GenerateCells()
        {
            Cells = new Cell[GridHeight, GridWidth];

            int rowCount = Cells.GetLength(0);
            int columnCount = Cells.GetLength(1);

            for (int x = 0; x < rowCount; x++)
            {
                for (int y = 0; y < columnCount; y++)
                {
                    Cell current = new Cell();
                    Cells[x, y] = current;
                    CellBag.Add(current);

                    switch (x,y)
                    {
                        // RIGHT WALL OR MIDDLE
                        case var tuple when tuple.x > 0 && tuple.y > 0 && tuple.y < columnCount - 1:
                            current.Neighbors.AddRange(new List<Cell>() {
                                Cells[x, y - 1],
                                Cells[x - 1, y - 1],
                                Cells[x - 1, y],
                                Cells[x - 1, y + 1],
                            });
                            Cells[x, y - 1].Neighbors.Add(current);
                            Cells[x - 1, y - 1].Neighbors.Add(current);
                            Cells[x - 1, y].Neighbors.Add(current);
                            Cells[x - 1, y + 1].Neighbors.Add(current);
                            break;

                        // BOTTOM WALL
                        case var tuple when tuple.x > 0 && tuple.x <= rowCount - 1 && tuple.y == columnCount - 1:
                            current.Neighbors.AddRange(new List<Cell>() {
                                Cells[x, y - 1],
                                Cells[x - 1, y - 1],
                                Cells[x - 1, y],
                            });
                            Cells[x, y - 1].Neighbors.Add(current);
                            Cells[x - 1, y - 1].Neighbors.Add(current);
                            Cells[x - 1, y].Neighbors.Add(current);
                            break;

                        // TOP WALL AND TOP RIGHT CORNER
                        case var tuple when tuple.x > 0  && tuple.y == 0:
                            current.Neighbors.AddRange(new List<Cell>() {
                                Cells[x - 1, y],
                                Cells[x - 1, y + 1]
                            });
                            Cells[x - 1, y].Neighbors.Add(current);
                            Cells[x - 1, y + 1].Neighbors.Add(current);
                            break;

                        // LEFT WALL
                        case var tuple when tuple.x == 0 && tuple.y >= 1 && tuple.y < columnCount - 1:
                            current.Neighbors.AddRange(new List<Cell>() {
                                Cells[x, y - 1],
                            });
                            Cells[x, y - 1].Neighbors.Add(current);
                            break;

                        // TOP LEFT Corner
                        case var tuple when tuple.x == 0 && tuple.y == 0:
                            // Do Nothing. First Cell in Network
                            break;

                        // BOTTOM LEFT Corner
                        case var tuple when tuple.x == 0 && tuple.y == columnCount - 1:
                            current.Neighbors.AddRange(new List<Cell>() { 
                                Cells[x, y-1]
                            });
                            Cells[x, y - 1].Neighbors.Add(current);
                            break;

                        // BOTTOM RIGHT Corner
                        case var tuple when tuple.x == rowCount - 1 && tuple.y == columnCount - 1:
                            current.Neighbors.AddRange(new List<Cell>() { 
                                Cells[x, y - 1],
                                Cells[x - 1, y - 1],
                                Cells[x - 1, y],
                            });
                            Cells[x, y - 1].Neighbors.Add(current);
                            Cells[x - 1, y - 1].Neighbors.Add(current);
                            Cells[x - 1, y].Neighbors.Add(current);
                            break;
                    }
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
            ConcurrentBag<Cell> deadCells = new ConcurrentBag<Cell>();
            ConcurrentBag<Cell> liveCells = new ConcurrentBag<Cell>();

            Parallel.ForEach(CellBag, cell =>
            {
                int livingNeighbors = cell.Neighbors.Where(n => n.Alive).Count();
                bool isLiving = cell.Alive;

                if (isLiving || livingNeighbors > 1)
                {
                    if ((!isLiving && livingNeighbors == 3) || (livingNeighbors == 2 || livingNeighbors == 3))
                    {
                        liveCells.Add(cell);
                    }
                    else
                    {
                        deadCells.Add(cell);
                    }
                }
            });

            foreach (Cell cell in deadCells)
            {
                cell.Alive = false;
            }

            foreach (Cell cell in liveCells)
            {
                cell.Alive = true;
            }
            GameClient.RefreshClient();
        }
    }
}
