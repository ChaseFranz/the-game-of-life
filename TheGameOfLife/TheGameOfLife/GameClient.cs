using System;
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
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TheGameOfLife
{
    public partial class GameClient : Form, IClient
    {
        public bool IsReady { get; set; }
        public IGameEngine GameEngine { get; set; }

        private Dictionary<Cell,(int x, int y, int width, int height)> CellPixelMapper { get; set; }

        public GameClient(IGameEngine gameEngine)
        {
            InitializeComponent();
            GameEngine = gameEngine;
            GameEngine.GameClient = this;
        }

        private void GameClient_Load(object sender, EventArgs e)
        {
            canvas.Size = new Size(ClientRectangle.Height, ClientRectangle.Height);
            canvas.Image = new Bitmap(canvas.Width, canvas.Height, PixelFormat.Format24bppRgb);
            IsReady = true;
            gameTimer.Enabled = true;
            GameEngine.StartGame();
        }

        public void UpdateClient()
        {
            for (int x = 0; x < GameEngine.Cells.GetLength(0); x++)
            {
                for (int y = 0; y < GameEngine.Cells.GetLength(1); y++)
                {
                    UpdateBitMap(GameEngine.Cells[x, y]);
                }
            }
            this.Refresh();
        }

        public void UpdateClient(ICollection<Cell> updateCells)
        {
            foreach(Cell cell in updateCells)
            {
                UpdateBitMap(cell);
            }

            this.Refresh();
        }

        private void UpdateBitMap(Cell cell)
        {
            Bitmap source = (Bitmap)canvas.Image;

            CellPixelMapper.TryGetValue(cell, out (int xCoordinate, int yCoordinate, int width, int height) value);

            for (int x = value.xCoordinate; x < value.xCoordinate + value.width; x++)
            {
                for(int y = value.yCoordinate; y < value.yCoordinate + value.height; y++)
                {
                    if (cell.Alive)
                    {
                        source.SetPixel(x, y, Color.Green);
                    }
                    else
                    {
                        source.SetPixel(x, y, Color.Black);
                    }
                }
            }
        }

        public void MapCellsToClient(Cell[,] cells)
        {
            int dimensionLength = (int)(Math.Sqrt(GameEngine.Cells.Length));

            // Get Dimensions in pixels of each cell
            int cellWidthPixels = (int)(canvas.Width / dimensionLength);
            int cellHeightPixels = (int)(canvas.Height / dimensionLength);
            CellPixelMapper = new Dictionary<Cell, (int x, int y, int width, int height)>();

            for (int x = 0; x < cells.GetLength(0); x++)
            {
                for (int y = 0; y < cells.GetLength(1); y++)
                {
                    CellPixelMapper.Add(cells[x, y], (x * cellWidthPixels, y * cellHeightPixels, cellWidthPixels, cellHeightPixels));
                }
            }
        }

        private void gameTimer_Tick(object sender, EventArgs e)
        {
            GameEngine.NextCycle();
        }
    }
}
