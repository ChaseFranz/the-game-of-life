using System;
using System.Collections.Concurrent;
using System.Drawing;
using System.Drawing.Imaging;
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

            for(int x=0; x < 10000; x++)
            {
                GameEngine.NextCycle();
            }
        }

        public void RefreshClient()
        {
            UpdateClientBitMapMultiThreadLockbits();
            Refresh();
        }

        private void UpdateClientBitMapSetPixel()
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
                            source.SetPixel(x, y, Color.White);
                        }
                        else
                        {
                            source.SetPixel(x, y, Color.DarkGray);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// http://csharpexamples.com/fast-image-processing-c/#comment-123485
        /// </summary>
        private void UpdateClientBitMapMultiThreadLockbits()
        {
            unsafe
            {
                Bitmap processedBitmap = (Bitmap)canvas.Image;

                BitmapData bitmapData = processedBitmap.LockBits(new Rectangle(0, 0, processedBitmap.Width, processedBitmap.Height), ImageLockMode.ReadWrite, processedBitmap.PixelFormat);

                int bytesPerPixel = System.Drawing.Bitmap.GetPixelFormatSize(processedBitmap.PixelFormat) / 8;
                int heightInPixels = bitmapData.Height;
                int widthInBytes = bitmapData.Width * bytesPerPixel;
                byte* PtrFirstPixel = (byte*)bitmapData.Scan0;

                Parallel.For(0, heightInPixels, y =>
                {

                    // Need to find what cell this pixel belongs to.

                    byte* currentLine = PtrFirstPixel + (y * bitmapData.Stride);

                    int currentPixel = 0;

                    for (int x = 0; x < widthInBytes; x = x + bytesPerPixel)
                    {

                        // Calculate y cell index
                        int pixelsPerCellY = heightInPixels / GameEngine.Ycells;
                        int yIndex = y/pixelsPerCellY;

                        int pixelsPerCellX = widthInBytes/bytesPerPixel/GameEngine.Xcells;

                        int xIndex = (x/bytesPerPixel) / pixelsPerCellX;

                        bool alive = GameEngine.Cells[xIndex, yIndex].Alive;
                        currentPixel++;

                        if (alive)
                        {
                            currentLine[x] = (byte)Color.White.B;
                            currentLine[x + 1] = (byte)Color.White.G;
                            currentLine[x + 2] = (byte)Color.White.R;
                        }
                        else
                        {
                            currentLine[x] = (byte)Color.DarkGray.B;
                            currentLine[x + 1] = (byte)Color.DarkGray.G;
                            currentLine[x + 2] = (byte)Color.DarkGray.R;
                        }
                        
                    }
                });
                processedBitmap.UnlockBits(bitmapData);
            }
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
    }
}
