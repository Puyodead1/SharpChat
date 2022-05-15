using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ChatServer
{
    internal class Server
    {
        public IPAddress HOST;
        public int PORT;
        private bool Running = true;
        public TcpListener Listener;
        static readonly object _lock = new object();
        static readonly Dictionary<int, TcpClient> Clients = new Dictionary<int, TcpClient>();

        public Server()
        {
            // TODO: get host from args
            // TODO: get port from args
            HOST = IPAddress.Parse("127.0.0.1");
            PORT = 8888;

            Listener = new TcpListener(HOST, PORT);
            Listener.Start();

            while(Running)
            {
                TcpClient client = Listener.AcceptTcpClient();
                int index = Clients.Count + 1;
                lock (_lock) Clients.Add(index, client);
                Console.WriteLine("Connection");

                ThreadPool.QueueUserWorkItem(HandleClient, index);
            }

            //NetworkStream stream = client.GetStream();
            //byte[] buffer = new byte[client.ReceiveBufferSize];

            //int bytesRead = stream.Read(buffer, 0, buffer.Length);
            //string data = Encoding.ASCII.GetString(buffer, 0, bytesRead);
            //Console.WriteLine(data);

            //stream.Write(buffer, 0, bytesRead);
            //client.Close();
            //listener.Stop();
        }

        public void HandleClient(object obj)
        {
            int index = (int)obj;
            TcpClient client;

            lock (_lock) client = Clients[index];

            while (true)
            {
                NetworkStream stream = client.GetStream();
                byte[] buffer = new byte[client.ReceiveBufferSize];
                int size = stream.Read(buffer, 0, buffer.Length);

                if (size == 0)
                {
                    break;
                }

                string data = Encoding.ASCII.GetString(buffer, 0, size);
                Console.WriteLine(data);
            }

            lock (_lock) Clients.Remove(index);
            client.Client.Shutdown(SocketShutdown.Both);
            client.Close();
        }
    }
}
