using System;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Collections.Generic;
using System.Net;
using MySql.Data;
using MySql.Data.MySqlClient;
using System.Text;

namespace LaserTagServer
{
    class Server
    {
        //private TcpListener LISTENER;
        private IPAddress ADDRESS;
        private string SERVER;
        UdpClient SOCK;
        private IPEndPoint IPEP;
        private int PORT;
        private const int CLIENT_LIMIT = 5; // 5 concurrent clients
        private MySqlConnection DBCONNECTION;
        private bool RUNNING; 

        private Dictionary<int, string> OPERATIONS = new Dictionary<int, string>();

        public Server(int port)
        {
            SERVER = Dns.GetHostName();
            ADDRESS = IPAddress.Parse("10.0.0.74");
            PORT = port;
            IPEP = new IPEndPoint(ADDRESS, PORT);
            SOCK = new UdpClient(IPEP);

            OPERATIONS.Add(0, "KILL SERVER");
            OPERATIONS.Add(1, "Initializing Connection");
            OPERATIONS.Add(2, "Join Game");
            OPERATIONS.Add(3, "Death");

            RUNNING = true;
        }

        public void Run()
        {
            DBConnect();

            IPEndPoint client = new IPEndPoint(IPAddress.Any, 0);
            byte[] data = new byte[1024];
            byte[] response;

            while (RUNNING)
            {
                data = SOCK.Receive(ref client);
                Console.Write("Message received from {0} : ", client.ToString());
                Console.WriteLine(Encoding.ASCII.GetString(data, 0, data.Length));

                response = ParseData(data);
                Console.WriteLine("Responding with: {0}", Encoding.ASCII.GetString(response, 0, response.Length));
                SOCK.Send(response, response.Length, client);
            }

            DBDisconnect();
        }

        private byte[] ParseData(byte[] data)
        {
            string response = "INVALID OPERATION CODE";
            int key = -1;

            if (!Int32.TryParse(Encoding.ASCII.GetString(data), out key))
                return Encoding.ASCII.GetBytes(response);

            if( !OPERATIONS.ContainsKey(key) )
                return Encoding.ASCII.GetBytes(response);

            response = OPERATIONS[key];

            if (key == 0)
                RUNNING = false;

            return Encoding.ASCII.GetBytes(response);
        }

        private void DBConnect()
        {
            string server = "127.0.0.1";
            string port = "3306";
            string databaseName = "LaserTagDataBase";
            string username = "LaserTagServer";
            string password = "LaserTagServerPassword101";

            string connstring = string.Format("Server={0};Port={1};database={2};UID={3};password={4}",
                server, port, databaseName, username, password);
            Console.WriteLine("Data Base Connection : " + connstring);
            DBCONNECTION = new MySqlConnection(connstring);

            try
            {
                DBCONNECTION.Open();
                Console.WriteLine("Database Connection Open");
                DBCONNECTION.Close();
            }
            catch(Exception ex)
            {
                Console.WriteLine("Unable to connect to database: " + ex.Message);
            }
        }
        
        private void DBDisconnect()
        {
            DBCONNECTION.Close();
            Console.WriteLine("Disconnect Data Base");
        }
    }
}
