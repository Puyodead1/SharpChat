using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using ChatCommon;

namespace ChatServer
{
    internal class Server
    {
        public IPAddress HOST;
        public int PORT;
        private bool Running = true;
        public TcpListener Listener;
        static readonly object _lock = new object();
        static readonly Dictionary<int, ClientConnection> ClientConnections = new Dictionary<int, ClientConnection>();

        public Server()
        {
            // TODO: get host from args
            // TODO: get port from args
            HOST = IPAddress.Parse("127.0.0.1");
            PORT = 8888;

            Listener = new TcpListener(HOST, PORT);
            Listener.Start();
            Console.WriteLine($"Server listening on port {PORT}");

            while(Running)
            {
                TcpClient client = Listener.AcceptTcpClient();
                ClientConnection connection = new ClientConnection(client);
                int index = ClientConnections.Count + 1;
                lock (_lock) ClientConnections.Add(index, connection);
                Console.WriteLine($"Client connected: {((IPEndPoint)client.Client.RemoteEndPoint).Address}");

                ThreadPool.QueueUserWorkItem(HandleClient, index);
            }

            Listener.Stop();
        }

        public void HandleClient(object obj)
        {
            int index = (int)obj;
            ClientConnection clientConnection;
            TcpClient client;

            lock (_lock) clientConnection = ClientConnections[index];
            client = clientConnection.Client;

            while (true)
            {
                NetworkStream stream = client.GetStream();
                byte[] buffer = new byte[client.ReceiveBufferSize];
                int size = stream.Read(buffer, 0, buffer.Length);

                if (size == 0)
                {
                    break;
                }

                try
                {
                    Console.WriteLine($"Recieved {size} bytes");
                    Packet packet = Packet.Parser.ParseFrom(buffer, 0, size);
                    switch(packet.Op)
                    {
                        case Packet.Types.OPCode.HandshakeRequest:
                            HandleHandshakeRequest(clientConnection, client, packet);
                            break;
                        default:
                            Console.WriteLine($"Unknown OpCode {packet.Op}");
                            break;
                    }
                } catch(Exception ex)
                {
                    Console.WriteLine($"Failed to parse packet: {ex}");
                }
            }

            Console.WriteLine($"Client disconnected: {((IPEndPoint)client.Client.RemoteEndPoint).Address}");

            lock (_lock) ClientConnections.Remove(index);
            client.Client.Shutdown(SocketShutdown.Both);
            client.Close();
        }

        public void HandleHandshakeRequest(ClientConnection clientConnection, TcpClient client, Packet packet)
        {
            // get the handshake request packet
            HandshakeRequest handshakeRequest = HandshakeRequest.Parser.ParseFrom(packet.Data);

            // create a new RSA crypto service
            RSACryptoServiceProvider rsaCryptoService = new RSACryptoServiceProvider();
            // import rsa public key
            rsaCryptoService.ImportRSAPublicKey(handshakeRequest.PublicKey.ToByteArray(), out _);
            // verify the data with the signature
            bool verified = rsaCryptoService.VerifyData(packet.Data.ToByteArray(), SHA512.Create(), packet.Signature.ToByteArray());
            Console.WriteLine($"Data Verified?: {verified}");
        }
    }
}
