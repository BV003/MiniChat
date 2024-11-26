using MiniChat.Transmitting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniSocket.Client
{
    /// <summary>
    /// 请求结果事件参数
    /// </summary>
    public class RequestResultEventArgs : EventArgs
    {
        /// <summary>
        /// 获取请求结果
        /// </summary>
        public RequestResult Result { get; set; }

        /// <summary>
        /// 创建一个请求结果的事件参数
        /// </summary>
        /// <param name="requestResult">请求结果</param>
        public RequestResultEventArgs(RequestResult requestResult)
        {
            Result = requestResult;
        }
    }
}
