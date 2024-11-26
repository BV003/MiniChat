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
    /// 客户端对象集合
    /// </summary>
    class ClientCollection
    {
        private Dictionary<string, Client> _clients;

        /// <summary>
        /// 创建一个客户端对象集合
        /// </summary>
        public ClientCollection()
        {
            _clients = new Dictionary<string, Client>();
        }

        /// <summary>
        /// 添加一个客户端
        /// </summary>
        /// <param name="id">客户端唯一ID</param>
        /// <param name="ipEndPoint">客户端网络终结点</param>
        /// <param name="socket">客户端Socket</param>
        public bool AddClient(string id, IPEndPoint ipEndPoint, Socket socket)
        {
            if (!id?.Equals(string.Empty) == true && ipEndPoint != null && socket != null)
            {
                if (!_clients.ContainsKey(id))
                {
                    _clients.Add(id, new Client(ipEndPoint, socket));
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 移除一个客户端
        /// </summary>
        /// <param name="id">客户端唯一ID</param>
        public bool RemoveClient(string id)
        {
            if (id != null)
            {
                if (_clients.ContainsKey(id))
                {
                    _clients[id].RemoveClient();
                    _clients.Remove(id);
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 移除所有客户端
        /// </summary>
        public void RemoveClients()
        {
            foreach (Client client in _clients.Values)
            {
                client.RemoveClient();
            }
            _clients.Clear();
        }

        /// <summary>
        /// 添加一个端口
        /// </summary>
        /// <param name="id">客户端唯一ID</param>
        /// <param name="port">端口</param>
        /// <param name="socket">Socket对象</param>
        public bool AddPort(string id, int port, Socket socket)
        {
            if (id != null && socket != null)
            {
                if (_clients.ContainsKey(id))
                {
                    return _clients[id].AddPort(port, socket);
                }
            }
            return false;
        }

        /// <summary>
        /// 移除一个端口
        /// </summary>
        /// <param name="id">客户端唯一ID</param>
        /// <param name="port">端口</param>
        public bool RemovePort(string id, int port)
        {
            if (id != null)
            {
                if (_clients.ContainsKey(id))
                {
                    return _clients[id].RemovePort(port);
                }
            }
            return false;
        }

        /// <summary>
        /// 检查客户端是否存在
        /// </summary>
        /// <param name="id">客户端唯一ID</param>
        public bool ContainClient(string id) => _clients.ContainsKey(id);

        /// <summary>
        /// 获取客户端的网络终结点，若客户端不存在则返回null
        /// </summary>
        /// <param name="id">客户端唯一ID</param>
        public IPEndPoint GetClientIPEndPoint(string id)
        {
            if (id != null)
            {
                if (_clients.ContainsKey(id))
                {
                    return _clients[id].IPEndPoint;
                }
            }
            return null;
        }

        /// <summary>
        /// 获取客户端所有的网络终结点，若客户端不存在则返回null
        /// </summary>
        /// <param name="id">客户端唯一ID</param>
        public List<IPEndPoint> GetClientIPEndPoints(string id)
        {
            if (id != null)
            {
                if (_clients.ContainsKey(id))
                {
                    List<IPEndPoint> ipEndPoints = new List<IPEndPoint>();
                    foreach (int port in _clients[id].GetPorts())
                    {
                        ipEndPoints.Add(new IPEndPoint(_clients[id].IPEndPoint.Address, port));
                    }
                    return ipEndPoints;
                }
            }
            return null;
        }

        /// <summary>
        /// 获取客户端所有端口，若客户端不存在则返回null
        /// </summary>
        /// <param name="id">客户端唯一ID</param>
        public List<int> GetClientPorts(string id)
        {
            if (id != null)
            {
                if (_clients.ContainsKey(id))
                {
                    return _clients[id].GetPorts();
                }
            }
            return null;
        }

        /// <summary>
        /// 获取客户端所有Socket，若客户端不存在则返回null
        /// </summary>
        /// <param name="id">客户端唯一ID</param>
        public List<Socket> GetClientSockets(string id)
        {
            if (id != null)
            {
                if (_clients.ContainsKey(id))
                {
                    return _clients[id].GetSockets();
                }
            }
            return null;
        }

        /// <summary>
        /// 获取所有客户端的ID
        /// </summary>
        public List<string> GetClientIds()
        {
            List<string> ids = new List<string>();
            foreach (string id in _clients.Keys)
            {
                ids.Add(id);
            }
            return ids;
        }
    }
}
