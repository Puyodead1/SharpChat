using ChatCommon;
using Google.Protobuf;
using System.Net.Sockets;
using System.Text;
using System.Security.Cryptography;

namespace ChatClient
{
    internal class Client
    {
        public string HOST;
        public int PORT;
        public bool Running = true;

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

            RSACryptoServiceProvider rsaCryptoProvider = new RSACryptoServiceProvider(2048);
            byte[] privateKey = rsaCryptoProvider.ExportRSAPrivateKey();
            byte[] publicKey = rsaCryptoProvider.ExportRSAPublicKey();

            //string s;
            //while(!string.IsNullOrEmpty(s = Console.ReadLine()) && Running)
            //{
            //    byte[] buffer = Encoding.ASCII.GetBytes(s);
            //    stream.Write(buffer, 0, buffer.Length);
            //}

            // Create the handshake request packet
            HandshakeRequest handshakeRequest = new HandshakeRequest
            {
                EncryptionPreference = EncryptionPreference.Strict,
                ProtocolVersion = ProtocolVersion.Version1,
                PublicKey = ByteString.CopyFrom(publicKey),
            };
            // get bytes from packet
            byte[] data = handshakeRequest.ToByteArray();
            // sign the data
            byte[] signature = rsaCryptoProvider.SignData(data, SHA512.Create());
            // create the final packet
            Packet packet = new Packet
            {
                Op = Packet.Types.OPCode.HandshakeRequest,
                Data = ByteString.CopyFrom(data),
                Signature = ByteString.CopyFrom(signature)
            };
            // get bytes from packet
            byte[] packetData = packet.ToByteArray();
            // write the packet
            stream.Write(packetData, 0, packetData.Length);
            Console.WriteLine($"Sent {packetData.Length} bytes");
            

            client.Client.Shutdown(SocketShutdown.Send);
            thread.Join();
            stream.Close();
            client.Close();
            Console.WriteLine("Disconnected");
            Console.ReadLine();

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
