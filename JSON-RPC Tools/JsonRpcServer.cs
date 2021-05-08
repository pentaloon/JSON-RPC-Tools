using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace JSON_RPC_Tools
{
    public class JsonRpcServer : IDisposable
    {
        private bool disposedValue;

        public static JsonRpcServer Instance() => new JsonRpcServer();

        private JsonRpcServer()
        {
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                }
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
