using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniChatSocket.Server
{
    /// <summary>
    /// 服务器内部错误事件参数
    /// </summary>
    public class ServerErrorEventArgs : EventArgs
    {
        /// <summary>
        /// 获取服务器的异常类型
        /// </summary>
        public Exception Exception { get; }
        /// <summary>
        /// 获取服务器的错误消息
        /// </summary>
        public string ErrorMessage { get; }

        /// <summary>
        /// 创建一个服务器内部错误的事件参数
        /// </summary>
        /// <param name="exception">异常类型</param>
        /// <param name="errorMessage">错误消息</param>
        public ServerErrorEventArgs(Exception exception, string errorMessage)
        {
            Exception = exception;
            ErrorMessage = errorMessage;
        }
    }
}
