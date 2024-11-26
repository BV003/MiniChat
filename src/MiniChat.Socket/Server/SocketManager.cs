using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace MiniSocket.Client
{
    /// <summary>
    /// Socket管理器
    /// </summary>
    class SocketManager
    {
        private string _clientID;
        private IPEndPoint _serverIPEndPoint;

        /// <summary>
        /// 创建一个Socket管理器
        /// </summary>
        /// <param name="clientID">客户端唯一ID</param>
        /// <param name="serverIPEndPoint">服务器网络终结点</param>
        public SocketManager(string clientID, IPEndPoint serverIPEndPoint)
        {
            if (serverIPEndPoint.AddressFamily != AddressFamily.InterNetwork &&
                serverIPEndPoint.AddressFamily != AddressFamily.InterNetworkV6)
            {
                throw new ArgumentException("只支持IPv4或IPv6地址");
            }
            _clientID = clientID;
            _serverIPEndPoint = serverIPEndPoint;
        }

        /// <summary>
        /// 异步的创建一个连接
        /// </summary>
        public async Task<SocketResult> CreateConnectionAsync()
        {
            return await Task.Run<SocketResult>(() =>
            {
                Socket socket = new Socket(_serverIPEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                try
                {
                    socket.Connect(_serverIPEndPoint);
                    byte[] checkBuffer = new byte[1] { 0 };
                    socket.Send(Encoding.UTF8.GetBytes(_clientID));
                    if (socket.Poll(15000000, SelectMode.SelectRead))
                    {
                        socket.Receive(checkBuffer);
                        if (checkBuffer[0] != 1)
                        {
                            return new SocketResult(null, null, ConnectionState.Failed);
                        }
                    }
                    else
                    {
                        return new SocketResult(null, null, ConnectionState.Timeout);
                    }
                }
                catch (Exception exception)
                {
                    return new SocketResult(null, exception, ConnectionState.Error);
                }
                return new SocketResult(socket, null, ConnectionState.Success);
            });
        }

        /// <summary>
        /// 关闭一个连接
        /// </summary>
        /// <param name="socket">Socket对象</param>
        public void CloseConnection(Socket socket)
        {
            try
            {
                socket.Shutdown(SocketShutdown.Both);
                socket.Close();
                socket.Dispose();
            }
            catch { }
            finally
            {
                socket = null;
            }
        }
    }
}
