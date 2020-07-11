using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TheGameOfLife
{
    public partial class GameClient : Form, IClient
    {
        public IGameEngine GameEngine { get; set; }

        private ConcurrentDictionary<Cell,(int x, int y, int width, int height)> CellPixelMapper { get; set; }

        public GameClient(IGameEngine gameEngine)
        {
            InitializeComponent();
            GameEngine = gameEngine;
            GameEngine.GameClient = this;
        }

        private void GameClient_Load(object sender, EventArgs e)
        {
            canvas.Size = new Size(ClientRectangle.Width, ClientRectangle.Height);
            canvas.Image = new Bitmap(canvas.Width, canvas.Height, PixelFormat.Format24bppRgb);
            gameTimer.Enabled = true;
            GameEngine.Ycells = canvas.Image.Height;
            GameEngine.Xcells = canvas.Image.Width;
            GameEngine.StartGame();

            for(int x=0; x < 1000; x++)
            {
                GameEngine.NextCycle();
            }
        }

        public void RefreshClient()
        {
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            UpdateClientBitMapSingleThread();
            Refresh();
            stopWatch.Stop();
            TimeSpan ts = stopWatch.Elapsed;

            // Format and display the TimeSpan value.
            string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                ts.Hours, ts.Minutes, ts.Seconds,
                ts.Milliseconds / 10);
            Debug.Print($"RunTime - ClientUpdate {elapsedTime}\n\n");
        }

        private void UpdateClientBitMapSingleThread()
        {
            foreach (Cell cell in GameEngine.CellBag)
            {
                Bitmap source = (Bitmap)canvas.Image;

                CellPixelMapper.TryGetValue(cell, out (int xCoordinate, int yCoordinate, int width, int height) value);

                for (int x = value.xCoordinate; x < value.xCoordinate + value.width; x++)
                {
                    for (int y = value.yCoordinate; y < value.yCoordinate + value.height; y++)
                    {
                        if (cell.Alive)
                        {
                            source.SetPixel(x, y, Color.DarkRed);
                        }
                        else
                        {
                            source.SetPixel(x, y, Color.DarkGray);
                        }
                    }
                }

            }
        }

        private void UpdateClientBitMapMultiThread()
        {
            //foreach (Cell cell in GameEngine.CellBag)
            //{
            //    Bitmap source = (Bitmap)canvas.Image;

            //    CellPixelMapper.TryGetValue(cell, out (int xCoordinate, int yCoordinate, int width, int height) value);

            //    for (int x = value.xCoordinate; x < value.xCoordinate + value.width; x++)
            //    {
            //        for (int y = value.yCoordinate; y < value.yCoordinate + value.height; y++)
            //        {
            //            if (cell.Alive)
            //            {
            //                source.SetPixel(x, y, Color.DarkRed);
            //            }
            //            else
            //            {
            //                source.SetPixel(x, y, Color.DarkGray);
            //            }
            //        }
            //    }

            //}
        }

        public void MapCellsToClient(Cell[,] cells)
        {
            // Get Dimensions in pixels of each cell
            int cellWidthPixels = (int)(canvas.Image.Width / GameEngine.Xcells);
            int cellHeightPixels = (int)(canvas.Image.Height / GameEngine.Ycells);


            CellPixelMapper = new ConcurrentDictionary<Cell, (int x, int y, int width, int height)>();


            Parallel.For(0, cells.GetLength(0), (int x) => {
                for (int y = 0; y < cells.GetLength(1); y++)
                {
                    CellPixelMapper.TryAdd(cells[x, y], (x * cellWidthPixels, y * cellHeightPixels, cellWidthPixels, cellHeightPixels));
                }
            });
            //for (int x = 0; x < cells.GetLength(0); x++)
            //{
            //    for (int y = 0; y < cells.GetLength(1); y++)
            //    {
            //        CellPixelMapper.Add(cells[x, y], (x * cellWidthPixels, y * cellHeightPixels, cellWidthPixels, cellHeightPixels));
            //    }
            //}
        }

        private void gameTimer_Tick(object sender, EventArgs e)
        {
            //GameEngine.NextCycle();
        }
    }
}
