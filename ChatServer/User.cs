using System.Net.Sockets;

namespace ChatServer
{
    public struct User
    {
        public string Username;
        public int Id;
        public DateTime CreatedAt;
        public string PublicKey;
    }
}
