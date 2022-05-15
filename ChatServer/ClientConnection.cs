using System.Net.Sockets;

namespace ChatServer
{
    internal class ClientConnection
    {
        public TcpClient Client;
        public User User;

        public ClientConnection(TcpClient Client)
        {
            this.Client = Client;
        }
    }
}
