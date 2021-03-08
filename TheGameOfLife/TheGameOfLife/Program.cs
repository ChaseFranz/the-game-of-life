using System;
using System.Windows.Forms;

namespace TheGameOfLife
{
    static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            
            IClient client = new GameClient(new GameEngine());

            Application.Run((GameClient)client);

            
        }
    }
}
