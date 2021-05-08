using Microsoft.VisualStudio.Threading;
using StreamJsonRpc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace JsonRpcTools
{
    public class JsonRpcClient : IDisposable
    {
        private bool disposedValue;
        private readonly TcpClient m_TcpClient;
        private readonly JsonRpc m_JsonRpc;
        private readonly JoinableTaskFactory m_JoinableTask = new JoinableTaskFactory(new JoinableTaskContext());
        private readonly CancellationTokenSource m_cts = new CancellationTokenSource();

        public static JsonRpcClient Instance(string ipAddress = "10.36.60.162", int port = 6340) => new JsonRpcClient(ipAddress, port);

        private JsonRpcClient(string ipAddress, int port)
        {
            m_TcpClient = new TcpClient(AddressFamily.InterNetwork);
            m_TcpClient.Connect(IPAddress.Parse(ipAddress), port);
            m_JsonRpc = JsonRpc.Attach(m_TcpClient.GetStream());
        }

        public object RpcRequest(string method, object parameter /*, int timeout = 30000*/)
        {
            object response = null;
            //m_cts.CancelAfter(timeout);
            m_JoinableTask.Run(async delegate
            {
                //response = await m_JsonRpc.InvokeWithParameterObjectAsync<object>(method, parameter, m_cts.Token);
                response = await m_JsonRpc.InvokeAsync<object>(method, parameter);
            });
            return response;
        }

        public object RpcRequest(string method, object[] parameters 
            /*, int timeout = 30000*/)
        {
            object response = null;
            //m_cts.CancelAfter(timeout);
            m_JoinableTask.Run(async delegate
            {
                //response = await m_JsonRpc.InvokeWithParameterObjectAsync<object>(method, parameter, m_cts.Token);
                response = await m_JsonRpc.InvokeAsync<object>(method, parameters);
            });
            return response;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    m_cts.Dispose();
                    m_JsonRpc.Dispose();
                    m_TcpClient.Close();
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
