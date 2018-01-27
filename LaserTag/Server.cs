using System;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Configuration;
using System.Collections.Generic;
using System.Net;

namespace LaserTagServer
{
    class Server
    {
        private TcpListener LISTENER;
        private IPAddress ADDRESS;
        private string SERVER;
        private int PORT;
        private const int CLIENT_LIMIT = 5; // 5 concurrent clients

        // Tempary Table
        private Dictionary<int, string> DICT = new Dictionary<int, string>();

        public Server(int port)
        {
            SERVER = Dns.GetHostName();
            ADDRESS = Dns.GetHostAddresses(SERVER)[0];
            PORT = port;
            LISTENER = new TcpListener(ADDRESS, PORT);

            // Set tempary table
            DICT.Add(1, "Initializing Connection");
            DICT.Add(2, "Join Game");
            DICT.Add(3, "Death");
        }

        public void Run()
        {
            LISTENER.Start();
            Console.WriteLine("Server mouted, listening to PORT: {0}", PORT);

            for (int i = 0; i < CLIENT_LIMIT; i++)
            {
                Thread t = new Thread(new ThreadStart(Service));
                t.Start();
            }
        }

        private void Service()
        {
            while(true)
            {
                Socket soc = LISTENER.AcceptSocket();
                Console.WriteLine("Connected: {0}", soc.RemoteEndPoint);

                try
                {
                    Stream s = new NetworkStream(soc);
                    StreamReader sr = new StreamReader(s);
                    StreamWriter sw = new StreamWriter(s);
                    sw.AutoFlush = true;
                    sw.WriteLine("{0} Clients ", DICT.Count);
                    while(true)
                    {
                        int key = Convert.ToInt32(sr.ReadLine());
                        if (key == 0) break;
                        string value = DICT[key];
                        if (value == "" || value == null) value = "Invalid Status Type";
                        sw.WriteLine(value);
                    }
                }
                catch(Exception e)
                {
                    Console.WriteLine(e.Message);
                }

                Console.WriteLine("Disconnected: {0}", soc.RemoteEndPoint);
                soc.Close();
            }
        }
    }
}
