using System;
using System.IO;
using System.Net.Sockets;
using System.Threading;
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
        private Dictionary<int, string> OPERATIONS = new Dictionary<int, string>();

        public Server(int port)
        {
            SERVER = Dns.GetHostName();
            ADDRESS = Dns.GetHostAddresses(SERVER)[0];
            PORT = port;
            LISTENER = new TcpListener(ADDRESS, PORT);

            // Set tempary table
            OPERATIONS.Add(1, "Initializing Connection");
            OPERATIONS.Add(2, "Join Game");
            OPERATIONS.Add(3, "Death");
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
                    sw.WriteLine("{0} Operations ", OPERATIONS.Count);
                    while(true)
                    {
                        int key = Convert.ToInt32(sr.ReadLine());
                        if (key == 0)
                        {
                            sw.WriteLine("Disconnected");
                            break;
                        }

                        string value = "Invalid Operation";
                        if (OPERATIONS.ContainsKey(key))
                            value = OPERATIONS[key];
                        
                        sw.WriteLine(value);
                    }
                    s.Close();
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
