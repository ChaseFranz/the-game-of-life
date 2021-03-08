using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TheGameOfLife
{
    public partial class GameClient : Form, IClient
    {
        public IGameEngine GameEngine { get; set; }

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
            GameEngine.GridHeight = canvas.Image.Height;
            GameEngine.GridWidth = canvas.Image.Width;
            GameEngine.StartGame();

            for (int x = 0; x < 1000; x++)
            {
                GameEngine.NextCycle();
            }  
        }

        public void RefreshClient()
        {
            UpdateClientBitMapMultiThreadLockbits();
            Refresh();
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
                int bytesPerPixel = Image.GetPixelFormatSize(processedBitmap.PixelFormat) / 8;
                int heightInPixels = bitmapData.Height;
                int widthInBytes = bitmapData.Width * bytesPerPixel;
                byte* PtrFirstPixel = (byte*)bitmapData.Scan0;
                int pixelsPerCellY = heightInPixels / GameEngine.GridHeight;
                int pixelsPerCellX = widthInBytes / bytesPerPixel / GameEngine.GridWidth;

                Parallel.For(0, heightInPixels, y =>
                {
                    byte* currentLine = PtrFirstPixel + (y * bitmapData.Stride);
                    // Calculate y cell index
                    int row = y / pixelsPerCellY;

                    for (int x = 0; x < widthInBytes; x += bytesPerPixel)
                    {
                        int column = (x/bytesPerPixel)/pixelsPerCellX;
                        bool alive = GameEngine.Cells[row,column].Alive;

                        currentLine[x] = alive ? Color.DarkRed.B : Color.LightGray.B;
                        currentLine[x + 1] = alive ? Color.DarkRed.G : Color.LightGray.G;
                        currentLine[x + 2] = alive ? Color.DarkRed.R : Color.LightGray.R;
                    }
                });
                processedBitmap.UnlockBits(bitmapData);
            }
        }
    }
}
