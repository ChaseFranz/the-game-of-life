using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using System.Windows.Forms;

namespace TheGameOfLife
{
    public class GameEngine : IGameEngine
    {
        public  Cell[,] Cells { get; set; }

        public IClient GameClient { get; set; }

        public GameEngine()
        {
        }

        /// <summary>
        /// This method is responsible for generating a collection of cells before each game starts
        /// </summary>
        private void GenerateCells()
        {
            Cells = new Cell[500,500];

            for (int x = 0; x < Cells.GetLength(0); x++)
            {
                for (int y = 0; y < Cells.GetLength(1); y++)
                {
                    Cell current = new Cell();
                    Cells[x, y] = current;

                    switch (x,y)
                    {
                        // RIGHT WALL OR MIDDLE
                        case var tuple when tuple.x > 0 && tuple.y > 0 && tuple.y < Cells.GetLength(1) - 1:
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
                        case var tuple when tuple.x > 0 && tuple.x <= Cells.GetLength(0) - 1 && tuple.y == Cells.GetLength(1) - 1:
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
                        case var tuple when tuple.x == 0 && tuple.y >= 1 && tuple.y < Cells.GetLength(1) - 1:
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
                        case var tuple when tuple.x == 0 && tuple.y == Cells.GetLength(1) - 1:
                            current.Neighbors.AddRange(new List<Cell>() { 
                                Cells[x, y-1]
                            });
                            Cells[x, y - 1].Neighbors.Add(current);
                            break;

                        // BOTTOM RIGHT Corner
                        case var tuple when tuple.x == Cells.GetLength(0) - 1 && tuple.y == Cells.GetLength(1) - 1:
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
            GameClient.MapCellsToClient(Cells);
        }

        public void StartGame()
        {
            GenerateCells();
            GameClient.UpdateClient();
        }

        public void NextCycle()
        {
            List<Cell> deadCells = new List<Cell>();
            List<Cell> liveCells = new List<Cell>();

            for (int x = 0; x < Cells.GetLength(0); x++)
            {
                for (int y = 0; y < Cells.GetLength(1); y++)
                {
                    if (!Cells[x, y].Alive && Cells[x,y].Neighbors.Where(n=>n.Alive).Count() == 0)
                    {
                        continue;
                    }

                    int livingNeighbors = Cells[x, y].Neighbors.Where(n => n.Alive).Count();
                    bool isLiving = Cells[x, y].Alive;

                    if (isLiving && livingNeighbors == 2 || livingNeighbors == 3)
                    {
                        liveCells.Add(Cells[x, y]);
                    }
                    else if (!isLiving && livingNeighbors == 3)
                    {
                        liveCells.Add(Cells[x, y]);
                    }
                    else
                    {
                        deadCells.Add(Cells[x, y]);
                    }
                }
            }

            foreach(Cell cell in deadCells)
            {
                cell.Alive = false;
            }

            foreach (Cell cell in liveCells)
            {
                cell.Alive = true;
            }

            GameClient.UpdateClient();
        }

        public void EndGame()
        {

        }
    }
}
