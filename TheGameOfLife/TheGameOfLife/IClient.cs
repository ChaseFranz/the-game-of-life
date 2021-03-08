namespace TheGameOfLife
{
    public interface IClient
    {
        public IGameEngine GameEngine { get; set; }

        public void RefreshClient();
 
    }
}