using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniSocket.Client
{
    /// <summary>
    /// 接收文件事件参数
    /// </summary>
    public class ReceiveFileEventArgs : EventArgs
    {
        /// <summary>
        /// 获取发送源ID
        /// </summary>
        public string SourceID { get; }

        /// <summary>
        /// 获取文件完整名称
        /// </summary>
        public string FileName { get; }

        /// <summary>
        /// 获取文件总大小
        /// </summary>
        public double FileSize { get; }

        /// <summary>
        /// 获取接收的文件数据
        /// </summary>
        public byte[] FileData { get; }

        /// <summary>
        /// 获取接收的有效字节数
        /// </summary>
        public int EffectiveBytes { get; }

        /// <summary>
        /// 创建一个接收文件事件参数
        /// </summary>
        /// <param name="sourceID">发送源ID</param>
        /// <param name="fileName">文件完整名称</param>
        /// <param name="fileSize">文件总大小</param>
        /// <param name="fileData">文件数据</param>
        /// <param name="effectiveBytes">有效字节</param>
        public ReceiveFileEventArgs(string sourceID, string fileName, double fileSize, byte[] fileData, int effectiveBytes)
        {
            SourceID = sourceID;
            FileName = fileName;
            FileSize = fileSize;
            FileData = fileData;
            EffectiveBytes = effectiveBytes;
        }
    }
}
