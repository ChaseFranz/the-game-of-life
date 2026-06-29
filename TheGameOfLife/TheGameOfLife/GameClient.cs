using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Threading.Tasks;
using System.Windows.Forms;
using TheGameOfLife.Core;

namespace TheGameOfLife
{
    public partial class GameClient : Form
    {
        /// <summary>Edge length, in pixels, of one simulation cell on screen.</summary>
        private const int CellSize = 4;

        /// <summary>Milliseconds between generations.</summary>
        private const int TickIntervalMs = 50;

        private IGameEngine _engine = null!;

        public GameClient()
        {
            InitializeComponent();
        }

        private void GameClient_Load(object sender, EventArgs e)
        {
            // Grid resolution is derived from the window size and the desired cell
            // size, independent of one-cell-per-pixel. The bitmap is sized to an
            // exact multiple of CellSize so every pixel maps to a valid cell.
            int columns = Math.Max(1, ClientRectangle.Width / CellSize);
            int rows = Math.Max(1, ClientRectangle.Height / CellSize);
            int bitmapWidth = columns * CellSize;
            int bitmapHeight = rows * CellSize;

            canvas.Size = new Size(bitmapWidth, bitmapHeight);
            canvas.Image = new Bitmap(bitmapWidth, bitmapHeight, PixelFormat.Format24bppRgb);

            _engine = new GameEngine(new GameConfig { Rows = rows, Columns = columns });
            _engine.GenerationAdvanced += OnGenerationAdvanced;
            _engine.StartGame();

            // Drive the simulation from the timer instead of a blocking loop, so
            // the UI stays responsive and actually animates.
            gameTimer.Interval = TickIntervalMs;
            gameTimer.Tick += (_, _) => _engine.NextCycle();
            gameTimer.Enabled = true;
        }

        private void OnGenerationAdvanced(object? sender, GenerationEventArgs e)
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
                Bitmap processedBitmap = (Bitmap)canvas.Image!;
                BitmapData bitmapData = processedBitmap.LockBits(new Rectangle(0, 0, processedBitmap.Width, processedBitmap.Height), ImageLockMode.ReadWrite, processedBitmap.PixelFormat);
                int bytesPerPixel = Image.GetPixelFormatSize(processedBitmap.PixelFormat) / 8;
                int heightInPixels = bitmapData.Height;
                int widthInBytes = bitmapData.Width * bytesPerPixel;
                byte* PtrFirstPixel = (byte*)bitmapData.Scan0;
                int pixelsPerCellY = heightInPixels / _engine.Rows;
                int pixelsPerCellX = widthInBytes / bytesPerPixel / _engine.Columns;

                Parallel.For(0, heightInPixels, y =>
                {
                    byte* currentLine = PtrFirstPixel + (y * bitmapData.Stride);
                    // Calculate y cell index
                    int row = y / pixelsPerCellY;

                    for (int x = 0; x < widthInBytes; x += bytesPerPixel)
                    {
                        int column = (x / bytesPerPixel) / pixelsPerCellX;
                        bool alive = _engine.Cells[row, column].Alive;

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
