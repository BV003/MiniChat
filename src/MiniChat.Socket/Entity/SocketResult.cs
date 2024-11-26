using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace MiniSocket.Client
{
    /// <summary>
    /// Socket结果
    /// </summary>
    class SocketResult
    {
        /// <summary>
        /// 获取Socket对象
        /// </summary>
        public Socket Socket { get; }
        /// <summary>
        /// 获取异常类型
        /// </summary>
        public Exception Exception { get; }
        /// <summary>
        /// 获取Socket连接状态
        /// </summary>
        public ConnectionState State { get; }

        /// <summary>
        /// 创建一个Socket结果
        /// </summary>
        /// <param name="socket">Socket对象</param>
        /// <param name="exception">异常类型</param>
        /// <param name="state">连接状态</param>
        public SocketResult(Socket socket, Exception exception, ConnectionState state)
        {
            Socket = socket;
            Exception = exception;
            State = state;
        }
    }

    /// <summary>
    /// 连接状态
    /// </summary>
    enum ConnectionState : byte
    {
        /// <summary>
        /// 连接成功
        /// </summary>
        Success = 1,
        /// <summary>
        /// 连接失败
        /// </summary>
        Failed = 2,
        /// <summary>
        /// 连接超时
        /// </summary>
        Timeout = 3,
        /// <summary>
        /// 连接时发生错误
        /// </summary>
        Error = 4
    }
}
