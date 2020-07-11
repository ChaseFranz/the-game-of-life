using System.Collections.Generic;

namespace TheGameOfLife
{
    public interface IClient
    {
        public IGameEngine GameEngine { get; set; }

        public bool IsReady { get; set; }
        public void UpdateClient();
        public void UpdateClient(ICollection<Cell> cellUpdates);

        public void MapCellsToClient(Cell[,] cells);

    }
}