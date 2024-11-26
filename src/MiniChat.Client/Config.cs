using System;
using System.Text;
using System.IO;
using System.Net;
using System.Net.Sockets;
using MiniSocket.Client;
using System.Xml.Linq;
using System.Linq;

namespace MiniChat.Client
{
    /// <summary>
    /// 应用程序配置
    /// </summary>
    public static class Config
    {
        /// <summary>
        /// 获取客户端名称
        /// </summary>
        public const string Name = "迷你通讯";
        /// <summary>
        /// 获取或设置客户端对象
        /// </summary>
        public static MiniClient MiniClient;

        /// <summary>
        /// 获取日志目录名称
        /// </summary>
        public const string LogDirectoryName = "Log";
        /// <summary>
        /// 获取文件目录名称
        /// </summary>
        public const string FileDirectoryName = "File";
        /// <summary>
        /// 获取缓存目录名称
        /// </summary>
        public const string CacheDirectoryName = "Cache";

        /// <summary>
        /// 获取客户端地址族
        /// </summary>
        public const AddressFamily ClientAddressFamily = AddressFamily.InterNetwork;
        /// <summary>
        /// 获取客户端网络协议
        /// </summary>
        public const ProtocolType ClientAgreement = ProtocolType.Tcp;

        /// <summary>
        /// 获取服务器网络终结点
        /// </summary>
        public static IPEndPoint GetServerIPEndPoint()
        {
            string configFile = "client-config.xml";
            if (!File.Exists(configFile))
            {
                new XElement("MiniClient",
                    new XElement("Server", new XElement("IP", "0.0.0.0"), new XElement("Port", 0))).Save(configFile);
            }
            XElement miniClient = XElement.Load(configFile);
            try
            {
                string ip = miniClient.Descendants("Server").Select(xml => xml.Element("IP").Value).ToArray()[0];
                int port = Convert.ToInt32(miniClient.Descendants("Server").Select(xml => xml.Element("Port").Value).ToArray()[0]);
                return new IPEndPoint(IPAddress.Parse(ip), port);
            }
            catch
            {
                return IPEndPoint.Parse("0.0.0.0:0");
            }
        }

        /// <summary>
        /// 获取用户目录的绝对路径，并创建相应目录
        /// </summary>
        /// <param name="userID">用户ID</param>
        /// <param name="directoryName">目录名称</param>
        public static string GetUserDirectory(string userID, string directoryName)
        {
            StringBuilder directoryString = new StringBuilder(Directory.GetCurrentDirectory());
            directoryString.Append("\\");
            directoryString.Append("User");
            directoryString.Append("\\");
            directoryString.Append(userID);
            directoryString.Append("\\");
            directoryString.Append(directoryName);
            string path = directoryString.ToString();
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            return path;
        }
    }
}