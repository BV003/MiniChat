using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows;
using System.Windows.Resources;

namespace MiniComm.Client.Helper
{
    /// <summary>
    /// 客户端帮助类
    /// </summary>
    public static class ClientHelper
    {
        /// <summary>
        /// 原子锁，防止多个线程同时访问同一个资源
        /// </summary>
        public static readonly object LockObject = new object();

        /// <summary>
        /// 记录消息日志
        /// </summary>
        /// <param name="directory">目录</param>
        /// <param name="value">内容</param>
        public static void WriteMessageLog(string directory, string value)
        {
            lock (LockObject)
            {
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }
                using (StreamWriter sw = new StreamWriter(directory + "\\message.log", true, Encoding.UTF8))
                {
                    sw.WriteLine($"[{DateTime.Now}]{value}");
                    sw.Flush();
                }
            }
        }

        /// <summary>
        /// 记录错误日志
        /// </summary>
        /// <param name="directory">目录</param>
        /// <param name="exception">异常类型</param>
        /// <param name="value">内容</param>
        public static void WriteErrorLog(string directory, Exception exception, string value)
        {
            lock (LockObject)
            {
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }
                using (StreamWriter sw = new StreamWriter(directory + "\\error.log", true, Encoding.UTF8))
                {
                    sw.WriteLine($"[{DateTime.Now}]{exception.GetType()}：{exception.Message}<{value}>");
                    sw.Flush();
                }
            }
        }

        /// <summary>
        /// 加密
        /// </summary>
        /// <param name="value">需要加密的字符串</param>
        public static string Encryption(string value)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(value);
            Array.Reverse(buffer);
            StringBuilder sb = new StringBuilder();
            for (int index = 0; index < buffer.Length; index++)
            {
                sb.Append(buffer[index]);
                if (index < buffer.Length - 1)
                {
                    sb.Append("#");
                }
            }
            return sb.ToString();
        }

        /// <summary>
        /// 解密
        /// </summary>
        /// <param name="value">需要解密的字符串</param>
        public static string Decryption(string value)
        {
            string[] array = value.Split("#", StringSplitOptions.RemoveEmptyEntries);
            byte[] buffer = new byte[array.Length];
            for (int index = 0; index < array.Length; index++)
            {
                if (!byte.TryParse(array[index], out buffer[index]))
                {
                    return string.Empty;
                }
            }
            Array.Reverse(buffer);
            return Encoding.UTF8.GetString(buffer);
        }

        /// <summary>
        /// 从路径获取字节数组
        /// </summary>
        /// <param name="path">路径</param>
        public static byte[] GetBytes(string path)
        {
            if (File.Exists(path))
            {
                using (FileStream fileStream = new FileStream(path, FileMode.Open, FileAccess.Read))
                {
                    byte[] buffer = new byte[fileStream.Length];
                    fileStream.Read(buffer, 0, buffer.Length);
                    return buffer;
                }
            }
            return null;
        }
        
        /// <summary>
        /// 从资源获取字节数组
        /// </summary>
        /// <param name="uri">资源标识</param>
        public static byte[] GetBytes(Uri uri)
        {
            if (uri != null)
            {
                StreamResourceInfo streamResourceInfo = Application.GetResourceStream(uri);
                using (Stream stream = streamResourceInfo.Stream)
                {
                    byte[] buffer = new byte[stream.Length];
                    stream.Read(buffer, 0, buffer.Length);
                    return buffer;
                }
            }
            return null;
        }

        /// <summary>
        /// 裁剪图片
        /// </summary>
        /// <param name="imagePath">图片路径</param>
        /// <param name="outputPath">输出路径</param>
        /// <param name="width">裁剪后的宽度</param>
        /// <param name="height">裁剪后的高度</param>
        public static bool CutPicture(string imagePath, string outputPath, int width, int height)
        {
            if (File.Exists(imagePath) && Directory.Exists(Path.GetDirectoryName(outputPath)))
            {
                Bitmap bitmap = new Bitmap(imagePath);
                new Bitmap(bitmap, width, height).Save(outputPath);
                return true;
            }
            return false;
        }

        /// <summary>
        /// 自动裁剪图片
        /// </summary>
        /// <param name="imagePath">图片路径</param>
        /// <param name="outputPath">输出路径</param>
        public static bool AutoCutPicture(string imagePath, string outputPath)
        {
            if (File.Exists(imagePath) && Directory.Exists(Path.GetDirectoryName(outputPath)))
            {
                Bitmap bitmap = new Bitmap(imagePath);
                if (bitmap.Width > bitmap.Height)
                    new Bitmap(bitmap, 180, 100).Save(outputPath);
                else if (bitmap.Width < bitmap.Height)
                    new Bitmap(bitmap, 100, 130).Save(outputPath);
                else
                    new Bitmap(bitmap, 120, 120).Save(outputPath);
                return true;
            }
            return false;
        }

        /// <summary>
        /// 异步的等待任务执行完成。如果任务是正常完成返回true，否则返回false
        /// </summary>
        /// <param name="func">一个任务</param>
        /// <param name="seconds">等待的时间，-1为无限等待</param>
        public static async Task<bool> WaitAsync(Func<bool> func, double seconds)
        {
            if (func != null && seconds > -2)
            {
                return await Task.Run<bool>(() =>
                {
                    int time = 0;
                    while (!func.Invoke())
                    {
                        Task.Delay(100).Wait();
                        if (seconds == -1)
                        {
                            continue;
                        }
                        time++;
                        if (time >= seconds * 10)
                        {
                            return false;
                        }
                    }
                    return true;
                });
            }
            return false;
        }
    }
}