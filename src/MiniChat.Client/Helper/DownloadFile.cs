using System.Collections.Generic;
using System.IO;

namespace MiniComm.Client.Helper
{
    /// <summary>
    /// 提供将文件保存到本地的功能
    /// </summary>
    public class DownloadFile
    {
        private Dictionary<string, Progress> _progresses = new Dictionary<string, Progress>();
        private Dictionary<string, FileStream> _fileStreams = new Dictionary<string, FileStream>();

        /// <summary>
        /// 开启一个任务
        /// </summary>
        /// <param name="name">任务名称</param>
        /// <param name="dataSize">数据总大小</param>
        /// <param name="path">保存的路径</param>
        public void Begin(string name, double dataSize, string path)
        {
            if (!_progresses.ContainsKey(name) && !_fileStreams.ContainsKey(name))
            {
                _progresses.Add(name, new Progress(0.0, dataSize));
                _fileStreams.Add(name, new FileStream(path, FileMode.OpenOrCreate, FileAccess.Write));
            }
        }

        /// <summary>
        /// 执行任务，任务执行完成后会自动结束并返回true
        /// </summary>
        /// <param name="name">任务名称</param>
        /// <param name="data">数据包</param>
        /// <param name="count">数据长度</param>
        public bool Go(string name, byte[] data, int count)
        {
            if (_progresses.ContainsKey(name) && _fileStreams.ContainsKey(name))
            {
                _fileStreams[name].Write(data, 0, count);
                _fileStreams[name].Flush();
                if(_progresses[name].Add(count))
                {
                    End(name);
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 结束一个任务
        /// </summary>
        /// <param name="name">任务名称</param>
        public void End(string name)
        {
            if (_progresses.ContainsKey(name) && _fileStreams.ContainsKey(name))
            {
                _fileStreams[name].Close();
                _fileStreams[name].Dispose();
                _progresses.Remove(name);
                _fileStreams.Remove(name);
            }
        }
    }
}