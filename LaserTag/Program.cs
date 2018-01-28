using LaserTagServer;
using System.Threading;

namespace LaserTag
{
    class Program
    {
        static void Main(string[] args)
        {
            Thread serverThread = new Thread(new ThreadStart(RunServer));
            Thread clientThread1 = new Thread(new ThreadStart(RunClient));
            Thread clientThread2 = new Thread(new ThreadStart(RunClient));

            serverThread.Start();
            serverThread.Join();

            clientThread1.Start();

            clientThread1.Join();

            clientThread2.Start();
        }

        public static void RunServer()
        {
            Server server = new Server(8000);
            server.Run();
        }

        public static void RunClient()
        {
            Client client = new Client(8000);
            client.Run();
        }
    }
}
