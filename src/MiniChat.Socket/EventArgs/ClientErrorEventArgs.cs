using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniSocket.Client
{
    /// <summary>
    /// 客户端内部错误事件参数
    /// </summary>
    public class ClientErrorEventArgs : EventArgs
    {
        /// <summary>
        /// 获取客户端的异常类型
        /// </summary>
        public Exception Exception { get; }
        /// <summary>
        /// 获取客户端的错误消息
        /// </summary>
        public string ErrorMessage { get; }

        /// <summary>
        /// 创建一个客户端内部错误的事件参数
        /// </summary>
        /// <param name="exception">异常类型</param>
        /// <param name="errorMessage">错误消息</param>
        public ClientErrorEventArgs(Exception exception, string errorMessage)
        {
            Exception = exception;
            ErrorMessage = errorMessage;
        }
    }
}
