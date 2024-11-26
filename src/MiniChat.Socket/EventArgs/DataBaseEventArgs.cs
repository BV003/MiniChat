using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniChatSocket.Server
{
    public class DatabaseEventArgs : EventArgs
    {
        /// <summary>
        /// 获取一个操作
        /// </summary>
        public string Operation { get; set; }
        /// <summary>
        /// 获取模型对象
        /// </summary>
        public object Model { get; set; }

        /// <summary>
        /// 创建一个数据库的事件参数
        /// </summary>
        /// <param name="operation">表示一个操作的字符串</param>
        /// <param name="parameter">模型对象</param>
        public DatabaseEventArgs(string operation, object model)
        {
            Operation = operation;
            Model = model;
        }
    }
}
