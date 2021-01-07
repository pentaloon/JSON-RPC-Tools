using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace JSON_RPC_Tools
{
    public class JSON_RPC_Server
    {
        public static JSON_RPC_Server Instance() => new JSON_RPC_Server();

        private JSON_RPC_Server()
        {
        }
    }
}
