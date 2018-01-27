using LaserTagServer;
using System;
using System.Threading;

namespace LaserTag
{
    class Program
    {
        static void Main(string[] args)
        {            
            Thread serverThread = new Thread(new ThreadStart(RunServer));

            serverThread.Start();
        }

        public static void RunServer()
        {
            Server server = new Server(8000);
            server.Run();
        }
    }
}
