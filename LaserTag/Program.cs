using LaserTagServer;
using System.Threading;

namespace LaserTag
{
    class Program
    {
        private static int PORT = 8000;

        static void Main(string[] args)
        {
            Thread serverThread = new Thread(new ThreadStart(RunServer));

            serverThread.Start();
        }

        public static void RunServer()
        {
            Server server = new Server(PORT);
            server.Run();
        }
    }
}
