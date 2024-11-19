using MiniChat.Transmitting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace MiniChat.Server.Server
{
    public static class Serverbin
    {
        /// <summary>
        /// 原子锁，防止多个线程同时访问同一个资源
        /// </summary>
        private static readonly object lockObject = new object();

        /// <summary>
        /// 运行日志文件路径
        /// </summary>
        private static readonly string runtimeLogFilePath = Directory.GetCurrentDirectory() + "\\runtime.log";
        /// <summary>
        /// 错误日志文件路径
        /// </summary>
        private static readonly string errorLogFilePath = Directory.GetCurrentDirectory() + "\\error.log";
        /// <summary>
        /// 配置文件路径
        /// </summary>
        private static readonly string configFilePath = Directory.GetCurrentDirectory() + "\\server-config.xml";

        /// <summary>
        /// 创建配置文件
        /// </summary>
        public static void CreateConfigFile()
        {
            if (!File.Exists(configFilePath))
            {
                new XElement("MiniServer",
                    new XElement("Server", new XElement("IP", "0.0.0.0"), new XElement("Port", 0)),
                    new XElement("Database", new XElement("ConnectionString", ""))).Save(configFilePath);
            }
        }

        /// <summary>
        /// 获取服务器的网络终结点
        /// </summary>
        public static IPEndPoint GetServerIPEndPoint()
        {
            XElement miniServer = XElement.Load(configFilePath);
            string ip = miniServer.Descendants("Server").Select(xml => xml.Element("IP").Value).ToArray()[0];
            int port = Convert.ToInt32(miniServer.Descendants("Server").Select(xml => xml.Element("Port").Value).ToArray()[0]);
            return new IPEndPoint(IPAddress.Parse(ip), port);
        }

        /// <summary>
        /// 获取数据库连接字符串
        /// </summary>
        public static string GetDatabaseConnectionString()
            => XElement.Load(configFilePath).Descendants("Database").Select(xml => xml.Element("ConnectionString").Value).ToArray()[0];

        /// <summary>
        /// 输出运行时日志
        /// </summary>
        /// <param name="runtimeMessage">运行时信息</param>
        public static void OutputRuntimeLog(string runtimeMessage)
        {
            lock (lockObject)
            {
                using (StreamWriter streamWriter = new StreamWriter(runtimeLogFilePath, true, Encoding.UTF8))
                {
                    streamWriter.WriteLine($"[{DateTime.Now}]{runtimeMessage}");
                    streamWriter.Flush();
                }
            }
        }

        /// <summary>
        /// 输出错误日志
        /// </summary>
        /// <param name="ex">异常类型</param>
        /// <param name="errorMessage">错误信息</param>
        public static void OutputErrorLog(Exception ex, string errorMessage)
        {
            lock (lockObject)
            {
                using (StreamWriter streamWriter = new StreamWriter(errorLogFilePath, true, Encoding.UTF8))
                {
                    streamWriter.WriteLine($"[{DateTime.Now}]{ex.GetType()}：{ex.Message}<{errorMessage}>");
                    streamWriter.Flush();
                }
            }
        }

        /// <summary>
        /// 设置用户头像，返回头像保存的路径
        /// </summary>
        /// <param name="name">用户名</param>
        /// <param name="headIcon">头像</param>
        public static string SetUserHeadIcon(string name, byte[] headIcon)
        {
            lock (lockObject)
            {
                if (!name?.Equals(string.Empty) == true && headIcon != null)
                {
                    string directory = "Users\\" + name;
                    string path = directory + "\\head.png";
                    if (!Directory.Exists(directory))
                    {
                        Directory.CreateDirectory(directory);
                    }
                    using (FileStream fs = new FileStream(path, FileMode.OpenOrCreate, FileAccess.Write))
                    {
                        fs.Write(headIcon, 0, headIcon.Length);
                        fs.Flush();
                    }
                    return path;
                }
                return string.Empty;
            }
        }

        /// <summary>
        /// 获取用户头像
        /// </summary>
        /// <param name="path">头像路径</param>
        public static byte[] GetUserHeadIcon(string path)
        {
            lock (lockObject)
            {
                if (File.Exists(path))
                {
                    using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read))
                    {
                        byte[] buffer = new byte[fs.Length];
                        fs.Read(buffer, 0, buffer.Length);
                        return buffer;
                    }
                }
                return null;
            }
        }
    }
}
