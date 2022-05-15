using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ChatClient
{
    internal class Client
    {
        public string HOST;
        public int PORT;

        public Client()
        {
            HOST = "127.0.0.1";
            PORT = 8888;

            TcpClient client = new TcpClient();
            client.Connect(HOST, PORT);
            Console.WriteLine("Connected");
            NetworkStream stream = client.GetStream();
            Thread thread = new Thread((o) => Listen((TcpClient)o));

            thread.Start(client);

            string s;
            while(!string.IsNullOrEmpty(s = Console.ReadLine()))
            {
                byte[] buffer = Encoding.ASCII.GetBytes(s);
                stream.Write(buffer, 0, buffer.Length);
            }

            client.Client.Shutdown(SocketShutdown.Send);
            thread.Join();
            stream.Close();
            client.Close();
            Console.WriteLine("Disconnected");

            //byte[] bytes = Encoding.ASCII.GetBytes(data);
            //stream.Write(bytes, 0, bytes.Length);

            //byte[] buffer = new byte[client.ReceiveBufferSize];
            //int bytesRead = stream.Read(buffer, 0, buffer.Length);
            //string dataIn = Encoding.ASCII.GetString(buffer, 0, bytesRead);
            //Console.WriteLine(dataIn);
            //client.Close();
        }

        public void Listen(TcpClient client)
        {
            NetworkStream stream = client.GetStream();
            byte[] buffer = new byte[client.ReceiveBufferSize];
            int size;

            while((size = stream.Read(buffer, 0, buffer.Length)) > 0)
            {
                Console.WriteLine(Encoding.ASCII.GetString(buffer, 0, size));
            }
        }
    }
}
