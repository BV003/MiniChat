using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniSocket.Client
{
    /// <summary>
    /// 接收文本事件参数
    /// </summary>
    public class ReceiveTextEventArgs : EventArgs
    {
        /// <summary>
        /// 获取发送源ID
        /// </summary>
        public string SourceID { get; }

        /// <summary>
        /// 获取接收到的文本消息
        /// </summary>
        public string TextMessage { get; }

        /// <summary>
        /// 创建一个接收文本事件参数
        /// </summary>
        /// <param name="sourceID">发送源ID</param>
        /// <param name="textMessage">文本消息</param>
        public ReceiveTextEventArgs(string sourceID, string textMessage)
        {
            SourceID = sourceID;
            TextMessage = textMessage;
        }
    }
}
