using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Collections.ObjectModel;
using Microsoft.Win32;
using MiniChat.Client.Commands;
using MiniChat.Client.Services;
using MiniChat.Client.Views;
using MiniChat.Client.Models;
using MiniChat.Client.Helper;
using MiniSocket.Client;
using MiniChat.Transmitting;
using System.Threading.Tasks;
using System.Text.Json;

namespace MiniChat.Client.ViewModels
{
    public class HomeViewModel : NotificationObject
    {
        private IDataService _dataService;
        private DownloadFile _downloadFile;

        private bool sendFileCanExecute = true;
        private bool refreshCanExecute = true;

        public static Action<RequestResult> RequestResultAction;

        public DelegateCommand LoadUserCommand { get; }
        public DelegateCommand LoadFriendCommand { get; }
        public DelegateCommand<bool> ShowUserInfoCommand { get; }
        public DelegateCommand RemoveFriendCommand { get; }
        public DelegateCommand SendTextCommand { get; }
        public DelegateCommand SendImageCommand { get; }
        public DelegateCommand SendFileCommand { get; }
        public DelegateCommand RefreshCommand { get; }
        public DelegateCommand CloseCommand { get; }

        private UserModel _userModel;
        public UserModel UserModel
        {
            get { return _userModel; }
            set
            {
                _userModel = value;
                base.OnPropertyChanged(nameof(UserModel));
            }
        }

        private UserModel _selectedFriend;
        public UserModel SelectedFriend
        {
            get { return _selectedFriend; }
            set
            {
                _selectedFriend = value;
                base.OnPropertyChanged(nameof(SelectedFriend));
                UpdateMessageList();
            }
        }

        public string _message;
        public string Message
        {
            get { return _message; }
            set
            {
                _message = value;
                base.OnPropertyChanged(nameof(Message));
            }
        }

        public ObservableCollection<UserModel> FriendList { get; }
        public ObservableCollection<MessageModel> MessageList { get; }

        public Dictionary<string, List<MessageModel>> MessageRecord { get; }

        public HomeViewModel(IDataService dataService)
        {
            _dataService = dataService;
            _downloadFile = new DownloadFile();
            LoadUserCommand = new DelegateCommand(LoadUser);
            LoadFriendCommand = new DelegateCommand(LoadFriend);
            ShowUserInfoCommand = new DelegateCommand<bool>(ShowUserInfo);
            RemoveFriendCommand = new DelegateCommand(RemoveFriend);
            SendTextCommand = new DelegateCommand(SendText);
            SendImageCommand = new DelegateCommand(SendImage);
            SendFileCommand = new DelegateCommand(SendFile, SendFileCanExecute);
            RefreshCommand = new DelegateCommand(Refresh, RefreshCanExecute);
            CloseCommand = new DelegateCommand(Close);
            FriendList = new ObservableCollection<UserModel>();
            MessageList = new ObservableCollection<MessageModel>();
            MessageRecord = new Dictionary<string, List<MessageModel>>();
            Config.MiniClient.ClientError += MiniClient_ClientError;
            Config.MiniClient.ClientReceiveString += MiniClient_ClientReceiveString;
            Config.MiniClient.ClientReceiveImage += MiniClient_ClientReceiveImage;
            Config.MiniClient.ClientReceiveFile += MiniClient_ClientReceiveFile;
            Config.MiniClient.ClientRequestResult += MiniClient_ClientRequestResult;
            Load();
        }

        /// <summary>
        /// 加载数据
        /// </summary>
        private async void Load()
        {
            await LoadUserAsync();
            await LoadFriendAsync();
        }

        /// <summary>
        /// 异步的加载用户信息
        /// </summary>
        private async Task LoadUserAsync()
        {
            await ClientHelper.WaitAsync(() => RequestResultAction == null, -1);

            RequestResultAction = new Action<RequestResult>((RequestResult result) =>
            {
                if (result.Object is JsonElement jsonElement && jsonElement.ValueKind == JsonValueKind.Object)
                {
                    // 将 JsonElement 反序列化为 User 对象
                    User user = JsonSerializer.Deserialize<User>(jsonElement.GetRawText());
                    if (user != null)
                {
                    UserModel = _dataService.GetUserModel(user);
                }
                }
                
            });

            if (!Config.MiniClient.SendDatabaseRequest(new User() { UserName = Config.MiniClient.ClientID }, "GetUserInfo", null))
            {
                RequestResultAction = null;
                MessageBox.Show("加载用户数据时发生错误！", Config.Name, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// 异步的加载好友信息
        /// </summary>
        private async Task LoadFriendAsync()
        {
            await ClientHelper.WaitAsync(() => RequestResultAction == null, -1);

            RequestResultAction = new Action<RequestResult>((RequestResult result) =>
            {
                if (result.Object is JsonElement jsonElement && jsonElement.ValueKind == JsonValueKind.Array)
                {
                    // 将 JsonElement 反序列化为 User 对象
                    User[] users = JsonSerializer.Deserialize<User[]>(jsonElement.GetRawText());
                    if (users == null) return;
                
                Home.HomeWindow.Dispatcher.Invoke(() =>
                {
                    var messageRecord = CopyMessageRecord();
                    MessageRecord.Clear();
                    FriendList.Clear();

                    foreach (var user in users)
                    {
                        FriendList.Add(_dataService.GetUserModel(user));
                        MessageRecord.Add(user.UserName, new List<MessageModel>());
                        if (messageRecord.ContainsKey(user.UserName))
                        {
                            foreach (var messageModel in messageRecord[user.UserName])
                            {
                                MessageRecord[user.UserName].Add(messageModel);
                            }
                        }
                    }
                });
                }
            });

            if (!Config.MiniClient.SendDatabaseRequest(new User() { UserName = UserModel.UserName }, "GetFriends", null))
            {
                RequestResultAction = null;
                MessageBox.Show("加载好友数据时发生错误！", Config.Name, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// 加载用户信息
        /// </summary>
        private async void LoadUser()
        {
            await LoadUserAsync();
        }

        /// <summary>
        /// 加载好友信息
        /// </summary>
        private async void LoadFriend()
        {
            await LoadFriendAsync();
        }

        /// <summary>
        /// 拷贝消息记录
        /// </summary>
        private Dictionary<string, List<MessageModel>> CopyMessageRecord()
        {
            var messageRecordCache = new Dictionary<string, List<MessageModel>>();
            foreach (string name in MessageRecord.Keys)
            {
                messageRecordCache.Add(name, new List<MessageModel>());
                foreach (MessageModel messageModel in MessageRecord[name])
                {
                    messageRecordCache[name].Add(messageModel);
                }
            }
            return messageRecordCache;
        }

        /// <summary>
        /// 保存消息记录
        /// </summary>
        /// <param name="userName">用户名</param>
        /// <param name="messageModel">用户消息模型</param>
        private void SaveMessageRecord(string userName, MessageModel messageModel)
        {
            if (MessageRecord.ContainsKey(userName))
            {
                MessageRecord[userName].Add(messageModel);
            }
        }

        /// <summary>
        /// 更新消息列表
        /// </summary>
        private void UpdateMessageList()
        {
            if (SelectedFriend != null)
            {
                string name = SelectedFriend.UserName;
                MessageList.Clear();
                if (MessageRecord.ContainsKey(name))
                {
                    foreach (MessageModel messageModel in MessageRecord[name])
                    {
                        MessageList.Add(messageModel);
                    }
                }
            }
        }

        /// <summary>
        /// 显示用户信息
        /// </summary>
        /// <param name="value">True表示个人信息，False表示好友信息</param>
        private async void ShowUserInfo(bool value)
        {
            if (value)
            {
                if (UserModel == null) return;
                new UserInfo(UserModel, Visibility.Visible).Show();
                return;
            }

            if (SelectedFriend == null) return;

            string friendName = SelectedFriend.UserName;

            await ClientHelper.WaitAsync(() => RequestResultAction == null, -1);

            RequestResultAction = new Action<RequestResult>((RequestResult result) =>
            {
                if (result.Object is JsonElement jsonElement && jsonElement.ValueKind == JsonValueKind.Object)
                {
                    // 将 JsonElement 反序列化为 User 对象
                    User user = JsonSerializer.Deserialize<User>(jsonElement.GetRawText());
                    if (user != null)
                    {
                        Home.HomeWindow.Dispatcher.Invoke(() =>
                        {
                            new UserInfo(_dataService.GetUserModel(user), Visibility.Hidden).Show();
                        });
                        return;
                    }
                    MessageBox.Show("加载用户数据时发生错误！", Config.Name, MessageBoxButton.OK, MessageBoxImage.Error);
                }
            });

            if (!Config.MiniClient.SendDatabaseRequest(new User() { UserName = friendName }, "GetUserInfo", null))
            {
                RequestResultAction = null;
                MessageBox.Show("加载用户数据时发生错误！", Config.Name, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// 移除好友
        /// </summary>
        private void RemoveFriend()
        {
            if (UserModel != null && SelectedFriend != null)
            {
                UserModel user = UserModel;
                UserModel friend = SelectedFriend;
                if (RequestResultAction == null)
                {
                    RequestResultAction = new Action<RequestResult>((RequestResult result) =>
                    {
                        if (result.Success == true)
                        {
                            Home.HomeWindow.Dispatcher.Invoke(() =>
                            {
                                FriendList.Remove(friend);
                            });
                            MessageBox.Show($"好友 {friend.NickName}({friend.UserName}) 移除成功！ ", Config.Name, MessageBoxButton.OK, MessageBoxImage.Asterisk);
                            return;
                        }
                        MessageBox.Show("移除好友时发生错误！", Config.Name, MessageBoxButton.OK, MessageBoxImage.Error);
                    });
                    if (MessageBox.Show($"确定要移除好友 {friend.NickName}({friend.UserName}) 吗？", Config.Name, MessageBoxButton.OKCancel, MessageBoxImage.Question) == MessageBoxResult.OK)
                    {
                        if (!Config.MiniClient.SendDatabaseRequest(new User() { UserName = user.UserName }, "RemoveFriend", friend.UserName))
                        {
                            RequestResultAction = null;
                            MessageBox.Show("移除好友时发生错误！", Config.Name, MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                        return;
                    }
                    RequestResultAction = null;
                }
            }
        }

        /// <summary>
        /// 发送文本消息
        /// </summary>
        private async void SendText()
        {
            if (UserModel != null && SelectedFriend != null && Message != null)
            {
                if (!Message.Equals(string.Empty))
                {
                    UserModel user = UserModel;
                    UserModel friend = SelectedFriend;
                    string message = Message;
                    if (await Config.MiniClient.SendTextAsync(friend.UserName, message))
                    {
                        SaveMessageRecord(friend.UserName, new MessageModel() { Source = new UserModel() { UserName = user.UserName, NickName = "Me" }, Text = message });
                        UpdateMessageList();
                        Message = string.Empty;
                        return;
                    }
                    MessageBox.Show("提交时发生异常，发送失败！", Config.Name, MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        /// <summary>
        /// 发送图片
        /// </summary>
        private async void SendImage()
        {
            if (UserModel != null && SelectedFriend != null)
            {
                UserModel user = UserModel;
                UserModel friend = SelectedFriend;
                OpenFileDialog openImage = new OpenFileDialog();
                openImage.Title = "上传图片";
                openImage.Filter = "图片|*.jpg;*.png";
                if (openImage.ShowDialog() == true)
                {
                    string cachePath = Config.GetUserDirectory(user.UserName, Config.CacheDirectoryName);
                    string imagePath = $"{cachePath}\\{DateTime.Now.ToString("yyyyMMddHHmmss")}{Path.GetExtension(openImage.SafeFileName)}";
                    if (ClientHelper.AutoCutPicture(openImage.FileName, imagePath))
                    {
                        if (await Config.MiniClient.SendImageAsync(friend.UserName, imagePath))
                        {
                            SaveMessageRecord(friend.UserName, new MessageModel() { Source = new UserModel() { UserName = user.UserName, NickName = "Me" }, Image = imagePath });
                            UpdateMessageList();
                            return;
                        }
                        MessageBox.Show("提交时发生异常，发送失败！", Config.Name, MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        /// <summary>
        /// 检查发送文件功能是否可用
        /// </summary>
        private bool SendFileCanExecute() => sendFileCanExecute;

        /// <summary>
        /// 发送文件
        /// </summary>
        private async void SendFile()
        {
            if (UserModel != null && SelectedFriend != null)
            {
                UserModel user = UserModel;
                UserModel friend = SelectedFriend;
                OpenFileDialog openFile = new OpenFileDialog();
                openFile.Title = "上传文件";
                openFile.Filter = "所有文件|*.*";
                if (openFile.ShowDialog() == true)
                {
                    sendFileCanExecute = false;
                    if (await Config.MiniClient.SendFileAsync(friend.UserName, openFile.FileName))
                    {
                        sendFileCanExecute = true;
                        MessageBox.Show("文件上传成功！", Config.Name, MessageBoxButton.OK, MessageBoxImage.Asterisk);
                        return;
                    }
                    sendFileCanExecute = true;
                    MessageBox.Show("提交时发生异常，发送失败！", Config.Name, MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        /// <summary>
        /// 检查刷新功能是否可用
        /// </summary>
        private bool RefreshCanExecute() => refreshCanExecute;

        /// <summary>
        /// 刷新
        /// </summary>
        private async void Refresh()
        {
            refreshCanExecute = false;
            await LoadFriendAsync();
            await ClientHelper.WaitAsync(() => false, 5);
            refreshCanExecute = true;
        }

        /// <summary>
        /// 客户端关闭
        /// </summary>
        private void Close()
        {
            Config.MiniClient?.CloseClient();
        }

        private void MiniClient_ClientError(object sender, ClientErrorEventArgs e)
        {
            ClientHelper.WriteErrorLog(Config.GetUserDirectory(Config.MiniClient.ClientID, Config.LogDirectoryName), e.Exception, e.ErrorMessage);
        }

        private void MiniClient_ClientReceiveString(object sender, ReceiveTextEventArgs e)
        {
            Home.HomeWindow.Dispatcher.Invoke(() =>
            {
                SaveMessageRecord(e.SourceID, new MessageModel() { Source = new UserModel() { UserName = e.SourceID, NickName = "You" }, Text = e.TextMessage });
                UpdateMessageList();
            });
        }

        private void MiniClient_ClientReceiveImage(object sender, ReceiveFileEventArgs e)
        {
            string imagePath = Config.GetUserDirectory(Config.MiniClient.ClientID, Config.CacheDirectoryName) + $"\\_{e.FileName}";
            _downloadFile.Begin(e.FileName, e.FileSize, imagePath);
            if (_downloadFile.Go(e.FileName, e.FileData, e.EffectiveBytes))
            {
                Home.HomeWindow.Dispatcher.Invoke(() =>
                {
                    SaveMessageRecord(e.SourceID, new MessageModel() { Source = new UserModel() { UserName = e.SourceID, NickName = "You" }, Image = imagePath });
                    UpdateMessageList();
                });
            }
        }

        private void MiniClient_ClientReceiveFile(object sender, ReceiveFileEventArgs e)
        {
            string filePath = Config.GetUserDirectory(Config.MiniClient.ClientID, Config.FileDirectoryName) + $"\\{e.FileName}";
            _downloadFile.Begin(e.FileName, e.FileSize, filePath);
            if (_downloadFile.Go(e.FileName, e.FileData, e.EffectiveBytes))
            {
                MessageBox.Show("对方向你发送了一个文件哦，快去" + filePath + "查看吧。");
            }
        }

        private void MiniClient_ClientRequestResult(object sender, RequestResultEventArgs e)
        {
            lock (ClientHelper.LockObject)
            {
                if (RequestResultAction != null)
                {
                    RequestResultAction.Invoke(e.Result);
                    RequestResultAction = null;
                }
            }
        }
    }
}