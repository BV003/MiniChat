using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniSocket.Client
{
    /// <summary>
    /// 客户端消息通知事件参数
    /// </summary>
    public class ClientNotifyEventArgs : EventArgs
    {
        /// <summary>
        /// 获取客户端的消息
        /// </summary>
        public string Message { get; }

        /// <summary>
        /// 创建一个客户端消息通知事件参数
        /// </summary>
        /// <param name="message">客户端消息</param>
        public ClientNotifyEventArgs(string message)
        {
            Message = message;
        }
    }
}
