using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniChatSocket.Server
{
    /// <summary>
    /// 服务器消息通知事件参数
    /// </summary>
    public class ServerNotifyEventArgs : EventArgs
    {
        /// <summary>
        /// 获取服务器的消息
        /// </summary>
        public string Message { get; }

        /// <summary>
        /// 创建一个服务器消息通知的事件参数
        /// </summary>
        /// <param name="message">服务器消息</param>
        public ServerNotifyEventArgs(string message)
        {
            Message = message;
        }
    }
}
