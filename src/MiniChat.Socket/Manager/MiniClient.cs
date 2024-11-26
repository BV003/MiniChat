using MiniChat.Transmitting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MiniSocket.Client
{
    /// <summary>
    /// 迷你客户端
    /// </summary>
    public class MiniClient
    {
        /// <summary>
        /// 客户端Socket
        /// </summary>
        private Socket clientSocket;
        /// <summary>
        /// Socket管理器
        /// </summary>
        private SocketManager socketManager;

        /// <summary>
        /// 获取客户端唯一的ID
        /// </summary>
        public string ClientID { get; }
        /// <summary>
        /// 客户端地址族
        /// </summary>
        public AddressFamily ClientAddressFamily { get; }
        /// <summary>
        /// 获取客户端的协议
        /// </summary>
        public ProtocolType ClientAgreement { get; }
        /// <summary>
        /// 获取客户端的网络终结点
        /// </summary>
        public IPEndPoint ClientIPEndPoint { get; private set; }
        /// <summary>
        /// 获取客户端的状态
        /// </summary>
        public bool ClientState { get; private set; }

        /// <summary>
        /// 获取服务器的网络终结点
        /// </summary>
        public IPEndPoint ServerIPEndPoint { get; }
        private bool _isReceiving = true; // 控制接收循环的标志位
        /// <summary>
        /// 客户端消息通知
        /// </summary>
        public event EventHandler<ClientNotifyEventArgs> ClientMessage;
        /// <summary>
        /// 客户端接收到文本消息时触发
        /// </summary>
        public event EventHandler<ReceiveTextEventArgs> ClientReceiveString;
        /// <summary>
        /// 客户端接收到图片时触发
        /// </summary>
        public event EventHandler<ReceiveFileEventArgs> ClientReceiveImage;
        /// <summary>
        /// 客户端接收到文件时触发
        /// </summary>
        public event EventHandler<ReceiveFileEventArgs> ClientReceiveFile;
        /// <summary>
        /// 客户端内部发生错误时触发
        /// </summary>
        public event EventHandler<ClientErrorEventArgs> ClientError;
        /// <summary>
        /// 客户端请求结果
        /// </summary>
        public event EventHandler<RequestResultEventArgs> ClientRequestResult;

        /// <summary>
        /// 创建一个迷你客户端
        /// </summary>
        /// <param name="id">客户端ID</param>
        /// <param name="addressFamily">客户端地址族</param>
        /// <param name="agreement">客户端协议</param>
        /// <param name="serverIPEndPoint">服务器网络终结点</param>
        public MiniClient(string id, AddressFamily addressFamily, ProtocolType agreement, IPEndPoint serverIPEndPoint)
        {
            if (addressFamily != AddressFamily.InterNetwork && addressFamily != AddressFamily.InterNetworkV6)
            {
                throw new ArgumentException("只支持IPv4或IPv6地址");
            }
            if (agreement != ProtocolType.Tcp && agreement != ProtocolType.Udp)
            {
                throw new ArgumentException("只支持TCP或UDP协议");
            }
            socketManager = new SocketManager(id, serverIPEndPoint);
            ClientID = id;
            ClientAddressFamily = addressFamily;
            ClientAgreement = agreement;
            ClientState = false;
            ServerIPEndPoint = serverIPEndPoint;
        }

        /// <summary>
        /// 初始化客户端
        /// </summary>
        private void InitializationClient()
        {
            if (ClientAgreement == ProtocolType.Tcp)
            {
                clientSocket = new Socket(ClientAddressFamily, SocketType.Stream, ClientAgreement);
            }
            else if (ClientAgreement == ProtocolType.Udp)
            {
                throw new ArgumentException("UDP协议暂时不受支持");
            }
            ClientIPEndPoint = clientSocket.LocalEndPoint as IPEndPoint;
        }

        /// <summary>
        /// 开启客户端
        /// </summary>
        public void OpenClient()
        {
            if (!ClientState)
            {
                ClientState = true;
                InitializationClient();
                ClientMessage?.Invoke(this, new ClientNotifyEventArgs("客户端已启动"));
            }
        }

        /// <summary>
        /// 关闭客户端
        /// </summary>
        public void CloseClient()
        {
            if (ClientState)
            {
                ClientState = false;
                _isReceiving = false;
                try
                {
                    clientSocket.Shutdown(SocketShutdown.Both);
                    clientSocket.Close();
                    clientSocket.Dispose();
                }
                catch { }
                finally
                {
                    clientSocket = null;
                    ClientIPEndPoint = null;
                }
                ClientMessage?.Invoke(this, new ClientNotifyEventArgs("客户端已关闭"));
            }
        }

        /// <summary>
        /// 异步的连接服务器。如果连接成功返回true，否则返回false
        /// </summary>
        public async Task<bool> ConnectionServerAsync()
        {
            if (ClientState)
            {
                return await Task.Run<bool>(() =>
                {
                    ClientMessage?.Invoke(this, new ClientNotifyEventArgs("正在建立与服务器的连接"));
                    try
                    {
                        clientSocket.Connect(ServerIPEndPoint);
                        byte[] checkBuffer = new byte[1] { 0 };
                        clientSocket.Send(Encoding.UTF8.GetBytes(ClientID));
                        if (clientSocket.Poll(15000000, SelectMode.SelectRead))
                        {
                            clientSocket.Receive(checkBuffer);
                            if (checkBuffer[0] != 1)
                            {
                                ClientMessage?.Invoke(this, new ClientNotifyEventArgs("服务器拒绝连接"));
                                return false;
                            }
                        }
                        else
                        {
                            ClientMessage?.Invoke(this, new ClientNotifyEventArgs("服务器连接超时"));
                            return false;
                        }
                    }
                    catch (Exception exception)
                    {
                        ClientMessage?.Invoke(this, new ClientNotifyEventArgs("无法连接到服务器"));
                        ClientError?.Invoke(this, new ClientErrorEventArgs(exception, "无法连接到服务器"));
                        return false;
                    }
                    switch (ClientAgreement)
                    {
                        case ProtocolType.Tcp:
                            Task task = Task.Run(MainReceiver);
                            break;
                        case ProtocolType.Udp:
                            break;
                    }
                    ClientMessage?.Invoke(this, new ClientNotifyEventArgs("服务器连接成功"));
                    return true;
                });
            }
            return false;
        }

        /// <summary>
        /// 异步的发送文本消息。如果发送成功返回true，否则返回false
        /// </summary>
        /// <param name="targetUserID">目标用户ID</param>
        /// <param name="message">文本消息</param>
        public async Task<bool> SendTextAsync(string targetUserID, string message)
        {
            if (ClientState && targetUserID != null && message != null)
            {
                return await Task.Run<bool>(() =>
                {
                    byte[] buffer = BytesConvert.ObjectToBytes(new Transmit()
                    {
                        SourceID = ClientID,
                        TargetID = targetUserID,
                        DataType = DataType.Text,
                        Parameter = message
                    });
                    try
                    {
                        clientSocket.Send(buffer, buffer.Length, SocketFlags.None);
                        return true;
                    }
                    catch (Exception exception)
                    {
                        ClientError?.Invoke(this, new ClientErrorEventArgs(exception, "在发送文本消息时发生错误"));
                        return false;
                    }
                });
            }
            return false;
        }

        /// <summary>
        /// 异步的发送图片。如果发送成功返回true，否则返回false
        /// </summary>
        /// <param name="targetUserID">目标ID</param>
        /// <param name="imagePath">图片路径</param>
        public async Task<bool> SendImageAsync(string targetUserID, string imagePath)
        {
            if (ClientState && targetUserID != null && imagePath != null)
            {
                return await Task.Run<bool>(() =>
                {
                    FileStream fileStream = null; //文件流
                    byte[] dataBuffer = new byte[1024 * 1024]; //数据缓冲区
                    try
                    {
                        fileStream = new FileStream(imagePath, FileMode.Open, FileAccess.Read);
                        if (fileStream.Length == 0) { return false; }
                        byte[] buffer = BytesConvert.ObjectToBytes(new Transmit()
                        {
                            SourceID = ClientID,
                            TargetID = targetUserID,
                            DataType = DataType.Image,
                            Parameter = $"{Path.GetFileName(imagePath)};{fileStream.Length}"
                        });
                        clientSocket.Send(buffer, buffer.Length, SocketFlags.None);
                        Thread.Sleep(100);
                        while (true)
                        {
                            int readBytes = fileStream.Read(dataBuffer, 0, dataBuffer.Length);
                            if (readBytes <= 0) { break; }
                            clientSocket.Send(dataBuffer, readBytes, SocketFlags.None);
                            Thread.Sleep(10);
                        }
                    }
                    catch (Exception exception)
                    {
                        ClientError?.Invoke(this, new ClientErrorEventArgs(exception, "在发送图片时发生错误"));
                        return false;
                    }
                    finally
                    {
                        fileStream?.Close();
                        fileStream?.Dispose();
                        fileStream = null;
                    }
                    return true;
                });
            }
            return false;
        }

        /// <summary>
        /// 异步的发送文件。如果发送成功返回true，否则返回false
        /// </summary>
        /// <param name="targetUserID">目标ID</param>
        /// <param name="filePath">文件路径</param>
        public async Task<bool> SendFileAsync(string targetUserID, string filePath)
        {
            if (ClientState && targetUserID != null && filePath != null)
            {
                return await Task.Run<bool>(async () =>
                {
                    FileStream fileStream = null; //文件流
                    byte[] dataBuffer = new byte[1024 * 1024]; //数据缓冲区
                    try
                    {
                        fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
                        if (fileStream.Length == 0) { return false; }
                        byte[] buffer = BytesConvert.ObjectToBytes(new Transmit()
                        {
                            SourceID = ClientID,
                            TargetID = targetUserID,
                            DataType = DataType.File,
                            Parameter = $"{Path.GetFileName(filePath)};{fileStream.Length}"
                        });
                        SocketResult socketResult = await socketManager.CreateConnectionAsync();
                        switch (socketResult.State)
                        {
                            case ConnectionState.Success:
                                socketResult.Socket.Send(buffer, buffer.Length, SocketFlags.None);
                                break;
                            case ConnectionState.Failed:
                                throw new ArgumentException("服务器拒绝连接");
                            case ConnectionState.Timeout:
                                throw new ArgumentException("服务器连接超时");
                            case ConnectionState.Error:
                                throw socketResult.Exception;
                        }
                        Thread.Sleep(5000);
                        while (true)
                        {
                            int readBytes = fileStream.Read(dataBuffer, 0, dataBuffer.Length);
                            if (readBytes <= 0) { break; }
                            socketResult.Socket.Send(dataBuffer, readBytes, SocketFlags.None);
                            Thread.Sleep(10);
                        }
                    }
                    catch (Exception exception)
                    {
                        ClientError?.Invoke(this, new ClientErrorEventArgs(exception, "在发送文件时发生错误"));
                        return false;
                    }
                    finally
                    {
                        fileStream?.Close();
                        fileStream?.Dispose();
                        fileStream = null;
                    }
                    return true;
                });
            }
            return false;
        }

        /// <summary>
        /// 发送一个数据库操作请求。如果发送成功返回true，否则返回false
        /// </summary>
        /// <param name="userModel">用户对象</param>
        /// <param name="operation">操作类型</param>
        /// <param name="parameter">操作参数</param>
        public bool SendDatabaseRequest(User user, string operation, string parameter)
        {
            if (ClientState)
            {
                byte[] buffer = BytesConvert.ObjectToBytes(new Transmit()
                {
                    SourceID = ClientID,
                    TargetID = ClientID,
                    DataType = DataType.Request,
                    Parameter = $"Database;{operation},{parameter}",
                    Object = user
                });
                SocketAsyncEventArgs args = new SocketAsyncEventArgs();
                try
                {
                    args.SetBuffer(buffer, 0, buffer.Length);
                    clientSocket.SendAsync(args);
                    return true;
                }
                catch (Exception exception)
                {
                    ClientError?.Invoke(this, new ClientErrorEventArgs(exception, "在发送请求时发生错误"));
                }
                finally
                {
                    args.Dispose();
                }
            }
            return false;
        }

        /// <summary>
        /// 主接收器
        /// </summary>
        private void MainReceiver()
        {
            byte[] dataBuffer = new byte[1024 * 1024]; //数据缓冲区

            bool isFirstReceive = true; //是否为首次接收数据
            Transmit transmit = null; //传输对象
            double dataSize = 0.0; //数据总大小
            double receiveBytes = 0.0; //已接收的字节数

            while (_isReceiving)
            {
                int readBytes = 0;
                try
                {
                    if (clientSocket == null)
                    {
                        throw new InvalidOperationException("clientSocket is not initialized.");
                    }
                    if (clientSocket.Poll(-1, SelectMode.SelectRead))
                    {
                        if (!ClientState) { break; }
                        try
                        {
                            readBytes = clientSocket.Receive(dataBuffer, dataBuffer.Length, SocketFlags.None);
                            if (readBytes == 0)
                            {
                                CloseClient();
                                ClientMessage?.Invoke(this, new ClientNotifyEventArgs("服务器断开连接"));
                                break;
                            }
                        }
                        catch (Exception exception)
                        {
                            CloseClient();
                            ClientMessage?.Invoke(this, new ClientNotifyEventArgs("服务器断开连接"));
                            ClientError?.Invoke(this, new ClientErrorEventArgs(exception, "服务器意外的断开连接"));
                            break;
                        }
                    }
                }
                catch (ObjectDisposedException)
                {
                    // 处理 Socket 已被关闭或不可用的异常
                    ClientMessage?.Invoke(this, new ClientNotifyEventArgs("Socket 已被关闭或不可用"));
                }
                catch (InvalidOperationException ex)
                {
                    // 处理 clientSocket 未初始化的异常
                    ClientMessage?.Invoke(this, new ClientNotifyEventArgs($"发生错误：{ex.Message}"));
                }
                catch (SocketException se)
                {
                    // 处理 Socket 相关的异常
                    ClientMessage?.Invoke(this, new ClientNotifyEventArgs($"Socket 错误：{se.Message}"));
                }
                catch (Exception exception)
                {
                    // 处理其他可能的异常
                    ClientMessage?.Invoke(this, new ClientNotifyEventArgs($"发生未预料的错误：{exception.Message}"));
                    ClientError?.Invoke(this, new ClientErrorEventArgs(exception, "接收数据时发生异常"));
                }


                //首次接收数据
                if (isFirstReceive)
                {
                    isFirstReceive = false;
                    transmit = BytesConvert.BytesToObject(dataBuffer, readBytes, "MiniSocket.Transmitting") as Transmit;
                    if (transmit?.Parameter != null)
                    {
                        switch (transmit.DataType)
                        {
                            case DataType.Text:
                                ReceiveDataHandler(transmit, null, 0);
                                break;
                            case DataType.Image:
                                string[] parameters = transmit.Parameter.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                                if (parameters.Length == 2)
                                {
                                    if (double.TryParse(parameters[1], out dataSize)) { continue; }
                                }
                                break;
                            case DataType.Request:
                                ReceiveRequestHandler(transmit);
                                break;
                        }
                    }
                    ResetParameter(ref isFirstReceive, ref transmit, ref dataSize, ref receiveBytes);
                    continue;
                }

                receiveBytes += readBytes; //已接收到的数据累加

                //判断已接收到的数据是否有效
                if (receiveBytes > 0 && receiveBytes <= dataSize)
                {
                    ReceiveDataHandler(transmit, dataBuffer, readBytes);
                }
                //判断是否接收完成或数据是否有误
                if (receiveBytes == dataSize || (receiveBytes <= 0 || receiveBytes > dataSize))
                {
                    ResetParameter(ref isFirstReceive, ref transmit, ref dataSize, ref receiveBytes);
                    continue;
                }
            }
        }

        /// <summary>
        /// 辅助接收器
        /// </summary>
        /// <param name="socket">客户端Socket对象</param>
        private void SecondaryReceiver(Socket socket)
        {
            if (socket == null) { return; }

            byte[] dataBuffer = new byte[1024 * 1024]; //数据缓冲区

            bool isFirstReceive = true; //是否为首次接收数据
            Transmit transmit = null; //传输对象
            double dataSize = 0.0; //数据总大小
            double receiveBytes = 0.0; //已接收的字节数

            while (true)
            {
                int readBytes = 0;
                if (socket.Poll(15000000, SelectMode.SelectRead))
                {
                    if (!ClientState) { break; }
                    try
                    {
                        readBytes = socket.Receive(dataBuffer, dataBuffer.Length, SocketFlags.None);
                        if (readBytes == 0)
                        {
                            socketManager.CloseConnection(socket);
                            break;
                        }
                    }
                    catch
                    {
                        socketManager.CloseConnection(socket);
                        break;
                    }
                }
                else
                {
                    socketManager.CloseConnection(socket);
                    break;
                }

                //首次接收数据
                if (isFirstReceive)
                {
                    isFirstReceive = false;
                    transmit = BytesConvert.BytesToObject(dataBuffer, readBytes, "MiniSocket.Transmitting") as Transmit;
                    if (transmit?.Parameter != null)
                    {
                        string[] parameters = transmit.Parameter.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                        if (parameters.Length == 2)
                        {
                            if (double.TryParse(parameters[1], out dataSize)) { continue; }
                        }
                    }
                    socketManager.CloseConnection(socket);
                    break;
                }

                receiveBytes += readBytes; //已接收到的数据累加

                //判断已接收到的数据是否有效
                if (receiveBytes > 0 && receiveBytes <= dataSize)
                {
                    ReceiveDataHandler(transmit, dataBuffer, readBytes);
                }
                //判断是否接收完成或数据是否有误
                if (receiveBytes == dataSize || (receiveBytes <= 0 || receiveBytes > dataSize))
                {
                    socketManager.CloseConnection(socket);
                    break;
                }
            }
        }

        /// <summary>
        /// 接收请求处理
        /// </summary>
        /// <param name="transmit">传输对象</param>
        private async void ReceiveRequestHandler(Transmit transmit)
        {
            switch (transmit.Parameter)
            {
                case "Client;AddSocket":
                    SocketResult socketResult = await socketManager.CreateConnectionAsync();
                    if (socketResult != null)
                    {
                        switch (socketResult.State)
                        {
                            case ConnectionState.Success:
                                Task task = Task.Run(() => SecondaryReceiver(socketResult.Socket));
                                break;
                            case ConnectionState.Failed:
                                break;
                            case ConnectionState.Timeout:
                                break;
                            case ConnectionState.Error:
                                break;
                        }
                    }
                    break;
                case "Client;RequestResult":
                    if (transmit.Object is JsonElement jsonElement && jsonElement.ValueKind == JsonValueKind.Object)
                    {
                        // 将 JsonElement 反序列化为 User 对象
                        RequestResult requestResult = JsonSerializer.Deserialize<RequestResult>(jsonElement.GetRawText());
                        if (requestResult != null)
                        {
                            ClientRequestResult?.Invoke(this, new RequestResultEventArgs(requestResult));
                        }
                        break;
                    }

                    break;
            }
        }

        /// <summary>
        /// 接收数据处理
        /// </summary>
        /// <param name="transmit">传输对象</param>
        /// <param name="data">数据包</param>
        /// <param name="effectiveBytes">数据有效字节</param>
        private void ReceiveDataHandler(Transmit transmit, byte[] data, int effectiveBytes)
        {
            string[] parameters = transmit.Parameter.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            switch (transmit.DataType)
            {
                case DataType.Text:
                    ClientReceiveString?.Invoke(this, new ReceiveTextEventArgs(transmit.SourceID, transmit.Parameter));
                    break;
                case DataType.Image:
                    ClientReceiveImage?.Invoke(this, new ReceiveFileEventArgs(transmit.SourceID, parameters[0], double.Parse(parameters[1]), data, effectiveBytes));
                    break;
                case DataType.File:
                    ClientReceiveFile?.Invoke(this, new ReceiveFileEventArgs(transmit.SourceID, parameters[0], double.Parse(parameters[1]), data, effectiveBytes));
                    break;
            }
        }

        private void ResetParameter(ref bool isFirstReceive, ref Transmit transmit, ref double dataSize, ref double receiveBytes)
        {
            isFirstReceive = true;
            transmit = null;
            dataSize = 0.0;
            receiveBytes = 0.0;
        }
    }
}
