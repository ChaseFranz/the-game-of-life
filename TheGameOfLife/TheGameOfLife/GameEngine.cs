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

            for (int row = 0; row < rowCount; row++)
            {
                for (int column = 0; column < columnCount; column++)
                {
                    Cells[row, column] = new Cell();
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
                    bool isLiving = currentCell.Alive;

                    currentCell.KillCell = !((isLiving && (liveNeighborCount == 2 || liveNeighborCount == 3)) || (!isLiving && liveNeighborCount == 3));
                }
            });

            Parallel.For(0, rows, rowIndex =>
            {
                for (int columnIndex = 0; columnIndex < columns; columnIndex++)
                {
                    Cells[rowIndex, columnIndex].Alive = !Cells[rowIndex, columnIndex].KillCell;
                }
            });
        }
    }
}
