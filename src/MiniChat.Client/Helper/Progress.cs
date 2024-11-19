using System;

namespace MiniComm.Client.Helper
{
    /// <summary>
    /// 表示一个进度
    /// </summary>
    public class Progress
    {
        /// <summary>
        /// 获取当前进度
        /// </summary>
        public double Current { get; private set; }
        /// <summary>
        /// 获取总进度
        /// </summary>
        public double TotalProgress { get; }

        /// <summary>
        /// 创建一个进度对象
        /// </summary>
        /// <param name="current">当前进度</param>
        /// <param name="size">总进度</param>
        public Progress(double current, double totalProgress)
        {
            Current = current;
            TotalProgress = totalProgress;
        }

        /// <summary>
        /// 增加进度，若进度已完成则返回true
        /// </summary>
        /// <param name="count">增加的长度</param>
        public bool Add(int count)
        {
            if (Current > TotalProgress || Current + count > TotalProgress)
            {
                throw new ArgumentException("数据不符合预期");
            }
            Current += count;
            if (Current == TotalProgress)
            {
                return true;
            }
            return false;
        }
    }
}