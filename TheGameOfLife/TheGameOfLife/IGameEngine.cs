namespace TheGameOfLife
{
    public interface IGameEngine
    {
        public IClient GameClient { get; set; }

        public Cell[,] Cells { get; set; }

        public void StartGame();

        public void NextCycle();
    }
}