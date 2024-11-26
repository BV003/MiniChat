using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace MiniChatSocket.Server
{
    /// <summary>
    /// 客户端对象
    /// </summary>
    class Client
    {
        /// <summary>
        /// 原子锁，防止多个线程同时访问同一个资源
        /// </summary>
        private readonly object lockObject = new object();

        private List<int> _ports;
        private List<Socket> _sockets;

        /// <summary>
        /// 获取客户端的网络终结点
        /// </summary>
        public IPEndPoint IPEndPoint { get; private set; }

        /// <summary>
        /// 创建一个客户端对象
        /// </summary>
        /// <param name="ip">客户端的IP地址</param>
        /// <param name="port">客户端的端口</param>
        /// <param name="socket">客户端的Socket</param>
        public Client(IPEndPoint ipEndPoint, Socket socket)
        {
            if (ipEndPoint == null || socket == null)
            {
                throw new ArgumentException("object is null");
            }
            _ports = new List<int>();
            _sockets = new List<Socket>();
            _ports.Add(ipEndPoint.Port);
            _sockets.Add(socket);
            IPEndPoint = ipEndPoint;
        }

        /// <summary>
        /// 添加一个端口
        /// </summary>
        /// <param name="port">端口</param>
        /// <param name="socket">Socket对象</param>
        public bool AddPort(int port, Socket socket)
        {
            lock (lockObject)
            {
                if (socket != null)
                {
                    if (!_ports.Contains(port))
                    {
                        _ports.Add(port);
                        _sockets.Add(socket);
                        return true;
                    }
                }
                return false;
            }
        }

        /// <summary>
        /// 移除一个端口
        /// </summary>
        /// <param name="port">端口</param>
        public bool RemovePort(int port)
        {
            lock (lockObject)
            {
                if (_ports.Contains(port))
                {
                    int index = 0;
                    for (int i = 0; i < _ports.Count; i++)
                    {
                        if (_ports[i] == port)
                        {
                            index = i;
                            break;
                        }
                    }
                    _ports.RemoveAt(index);
                    try
                    {
                        _sockets[index].Shutdown(SocketShutdown.Both);
                        _sockets[index].Close();
                        _sockets[index].Dispose();
                    }
                    catch { }
                    finally
                    {
                        _sockets[index] = null;
                    }
                    _sockets.RemoveAt(index);
                    return true;
                }
                return false;
            }
        }

        /// <summary>
        /// 销毁客户端
        /// </summary>
        public void RemoveClient()
        {
            lock (lockObject)
            {
                for (int index = 0; index < _sockets.Count; index++)
                {
                    try
                    {
                        _sockets[index].Shutdown(SocketShutdown.Both);
                        _sockets[index].Close();
                        _sockets[index].Dispose();
                    }
                    catch { }
                    finally
                    {
                        _sockets[index] = null;
                    }
                }
            }
        }

        /// <summary>
        /// 获取客户端所有端口
        /// </summary>
        public List<int> GetPorts() => _ports;

        /// <summary>
        /// 获取客户端所有Socket
        /// </summary>
        public List<Socket> GetSockets() => _sockets;
    }
}
