using System;
using System.IO;
using System.Net;
using System.Net.Sockets;

namespace LaserTag
{
    class Client
    {
        private TcpClient CLIENT;
        private IPAddress ADDRESS;
        private string SERVER;
        private int PORT;

        public Client(int port)
        {
            SERVER = Dns.GetHostName();
            ADDRESS = Dns.GetHostAddresses(SERVER)[0]; 
            PORT = port;
            CLIENT = new TcpClient(ADDRESS.ToString(), PORT);

            Console.WriteLine(ADDRESS.ToString());
        }

        public void Run()
        {
            try
            {
                Stream s = CLIENT.GetStream();
                StreamReader sr = new StreamReader(s);
                StreamWriter sw = new StreamWriter(s);
                sw.AutoFlush = true;

                Console.WriteLine(sr.ReadLine());
                while(true)
                {
                    Console.WriteLine("Operation: ");
                    string operation = Console.ReadLine();
                    if (operation == "") operation = "0";

                    sw.WriteLine(operation);
                    string value = sr.ReadLine();
                    Console.WriteLine(value);

                    if (value == "Disconnected")
                        break;
                }
                s.Close(); 
            }
            finally
            {
                CLIENT.Close();
            }

            Console.WriteLine("Client has finished");
        }
    }
}
