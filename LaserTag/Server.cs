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
    struct ClientOperationData
    {
        public byte OPERATION_CODE;
        public byte[] OPERATION_DATA;

        public ClientOperationData(byte operationCode, byte[] operationData)
        {
            OPERATION_CODE = operationCode;
            OPERATION_DATA = operationData;
        }
    }

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

        public Server(int port)
        {
            SERVER = Dns.GetHostName();
            ADDRESS = IPAddress.Parse("10.0.0.74");
            PORT = port;
            IPEP = new IPEndPoint(ADDRESS, PORT);
            SOCK = new UdpClient(IPEP);

            InitializeDataBase();

            RUNNING = true;
        }

        public void Run()
        {
            Console.Write("Listening For Incomming Messages...");
            IPEndPoint client = new IPEndPoint(IPAddress.Any, 0);

            while (RUNNING)
            {
                byte[] data = SOCK.Receive(ref client);
                Console.Write("Message received from {0} : ", client.ToString());
                Console.WriteLine(Encoding.ASCII.GetString(data, 0, data.Length));

                ClientOperationData clientOperationData = ParseData(data);
                byte[] response = handleData(clientOperationData);

                Console.WriteLine("Responding with: {0}", Encoding.ASCII.GetString(response, 0, response.Length));
                SOCK.Send(response, response.Length, client);
            }
        }

        private byte[] handleData(ClientOperationData clientOpeartionData)
        {
            string response = "";

            switch (clientOpeartionData.OPERATION_CODE)
            {
                case (byte)'1': // connecting get username
                    string uniqueKey = Encoding.ASCII.GetString(clientOpeartionData.OPERATION_DATA);
                    string mysqlQuery = "SELECT username FROM clientuserdata WHERE unique_key = @uniquekey";

                    MySqlCommand cmd = new MySqlCommand(mysqlQuery, DBCONNECTION);
                    cmd.Parameters.AddWithValue("@uniquekey", uniqueKey);

                    string username = queryDatabase(cmd);

                    if(username != "")
                        response = "1" + username;

                    break;

                case (byte)'2': // join game
                    break;

                default:
                    response = "0";
                    break;
            }

            return Encoding.ASCII.GetBytes(response);
        }

        private string queryDatabase(MySqlCommand cmd)
        {
            string username = "";

            if (!DBConnect())
                return username;

            try
            {
                MySqlDataReader rdr = cmd.ExecuteReader();

                while (rdr.Read())
                {
                    username += rdr[0];
                }
                rdr.Close();
            }
            catch(Exception ex)
            {
                Console.WriteLine("FAIL TO QUERY THE DATABASE: " + ex.ToString());
            }

            DBDisconnect();
            return username;
        }

        private ClientOperationData ParseData(byte[] data)
        {
            byte operation = 0x00;
            byte[] operationData = new byte[data.Length - 1];

            if (data.Length < 1)
                return new ClientOperationData(operation, operationData);

            operation = data[0];
            for (int index = 1; index < data.Length; index++)
                operationData[index - 1] = data[index];
            
            return new ClientOperationData(operation, operationData);
        }

        private void InitializeDataBase()
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
        }

        private bool DBConnect()
        {
            try
            {
                DBCONNECTION.Open();
                Console.WriteLine("Database Connection Open");
                return true;
            }
            catch(Exception ex)
            {
                Console.WriteLine("Unable to connect to database: " + ex.Message);
            }

            return false;
        }
        
        private void DBDisconnect()
        {
            DBCONNECTION.Close();
            Console.WriteLine("Disconnect Data Base");
        }
    }
}
