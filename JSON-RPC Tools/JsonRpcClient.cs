using Microsoft.VisualStudio.Threading;
using StreamJsonRpc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NationalInstruments.TestStand.Interop.API;

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
            var formatter = new JsonMessageFormatter(Encoding.UTF8);
            var handler = new LengthHeaderMessageHandler(m_TcpClient.GetStream(), m_TcpClient.GetStream(), formatter);
            m_JsonRpc = new JsonRpc(handler);
            // Add any applicable target objects/methods here, or in the JsonRpc constructor above
            m_JsonRpc.StartListening();
        }

        //private JsonRpcClient(string ipAddress, int port)
        //{
        //    m_TcpClient = new TcpClient(AddressFamily.InterNetwork);
        //    m_TcpClient.Connect(IPAddress.Parse(ipAddress), port);
        //    m_JsonRpc = JsonRpc.Attach(m_TcpClient.GetStream());
        //}

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

        byte[] getBytes(object strct)
        {
            int size = Marshal.SizeOf(strct);
            byte[] arr = new byte[size];

            IntPtr ptr = Marshal.AllocHGlobal(size);
            Marshal.StructureToPtr(strct, ptr, true);
            Marshal.Copy(ptr, arr, 0, size);
            Marshal.FreeHGlobal(ptr);
            return arr;
        }

        public object RpcRequestByteArray(string method, object anything/*, int timeout = 30000*/)
        {
            object response = null;

            //m_cts.CancelAfter(timeout);
            m_JoinableTask.Run(async delegate
                {
                //response = await m_JsonRpc.InvokeWithParameterObjectAsync<object>(method, parameter, m_cts.Token);
                response = await m_JsonRpc.InvokeAsync<object>(method, getBytes(anything));
                });
            return response;
        }

        public object RpcRequestFromContainer(string method, PropertyObject ptyObj/*, int timeout = 30000*/)
        {
            object response = null;

            //m_cts.CancelAfter(timeout);
            m_JoinableTask.Run(async delegate
            {
                //response = await m_JsonRpc.InvokeWithParameterObjectAsync<object>(method, parameter, m_cts.Token);
                response = await m_JsonRpc.InvokeAsync<object>(method, getPropObjectValue(ptyObj));
            });
            return response;
        }

        private object getPropObjectValue(PropertyObject ptyObj)
        {
            var typeString = ptyObj.GetTypeDisplayString("", 0);
            if (typeString.StartsWith("Array"))
            {
                //add support for 2D arrays
                object[] ptyObjs = new object[ptyObj.GetNumElements()];
                for (int i = 0; i < ptyObjs.Length; i++)
                {
                    ptyObjs[i] = getPropObjectValue(ptyObj.GetPropertyObject(string.Format("[{0}]", i), 0));
                }
                return ptyObjs;
            }

            switch (typeString)
            {
                case "Boolean":
                    return ptyObj.GetValBoolean("", 0);
                case "String":
                    return ptyObj.GetValString("", 0);
                case "Number":
                    return ptyObj.GetValNumber("", 0);
                case "Container":
                    {
                        object[] ptyObjs = new object[ptyObj.GetNumSubProperties("")];
                        for (int i = 0; i < ptyObjs.Length; i++)
                        {
                            ptyObjs[i] = getPropObjectValue(ptyObj.GetNthSubProperty("", i, 0));
                        }
                        return ptyObjs;
                    }
                default:
                    throw new InvalidOperationException("unsupported data type");
            }
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
