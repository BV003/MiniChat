using System;
using System.Text.Json;
using System.Windows;
using MiniChat.Client.Commands;
using MiniChat.Client.Views;
using MiniSocket.Client;
using MiniChat.Transmitting;

namespace MiniChat.Client.ViewModels
{
    public class SeekFriendViewModel : NotificationObject
    {
        private Action<RequestResult> _requestResultAction;
        private bool addFriendCanExecute = true;

        public DelegateCommand AddFriendCommand { get; }
        public DelegateCommand CloseCommand { get; }

        private string _friendName;
        public string FriendName
        {
            get { return _friendName; }
            set
            {
                _friendName = value;
                base.OnPropertyChanged(nameof(FriendName));
            }
        }

        public SeekFriendViewModel()
        {
            AddFriendCommand = new DelegateCommand(AddFriend, AddFriendCanExecute);
            CloseCommand = new DelegateCommand(Close);
        }

        /// <summary>
        /// 检查添加好友功能是否可用
        /// </summary>
        private bool AddFriendCanExecute() => addFriendCanExecute;

        /// <summary>
        /// 添加好友
        /// </summary>
        private void AddFriend()
        {
            if (FriendName != null)
            {
                string friendName = FriendName.Trim();
                if (!friendName.Equals(string.Empty))
                {
                    addFriendCanExecute = false;
                    Config.MiniClient.ClientRequestResult += MiniClient_ClientRequestResult;
                    _requestResultAction = (RequestResult result) => QueryFriendAction(result, friendName);
                    if (!Config.MiniClient.SendDatabaseRequest(new User() { UserName = friendName }, "GetUserInfo", null))
                    {
                        Config.MiniClient.ClientRequestResult -= MiniClient_ClientRequestResult;
                        MessageBox.Show("查询好友时发生错误！", Config.Name, MessageBoxButton.OK, MessageBoxImage.Error);
                        addFriendCanExecute = true;
                        return;
                    }
                }
            }
        }

        /// <summary>
        /// 窗口关闭
        /// </summary>
        private void Close()
        {
            Config.MiniClient.ClientRequestResult -= MiniClient_ClientRequestResult;
        }

        private void QueryFriendAction(RequestResult result, string friendName)
        {
            if (result.Object is JsonElement jsonElement && jsonElement.ValueKind == JsonValueKind.Object)
            {
                // 将 JsonElement 反序列化为 User 对象
                User user = JsonSerializer.Deserialize<User>(jsonElement.GetRawText());
                if (user != null)
                {
                    if (MessageBox.Show($"确定要添加 {user.NickName}({user.UserName}) 为好友吗？", Config.Name, MessageBoxButton.OKCancel, MessageBoxImage.Question) == MessageBoxResult.OK)
                    {
                        _requestResultAction = (RequestResult result) => AddFriendAction(result, user);
                        if (!Config.MiniClient.SendDatabaseRequest(new User() { UserName = Config.MiniClient.ClientID }, "AddFriend", friendName))
                        {
                            Config.MiniClient.ClientRequestResult -= MiniClient_ClientRequestResult;
                            MessageBox.Show("添加好友时发生错误！", Config.Name, MessageBoxButton.OK, MessageBoxImage.Error);
                            addFriendCanExecute = true;
                        }
                        return;
                    }
                    Config.MiniClient.ClientRequestResult -= MiniClient_ClientRequestResult;
                    addFriendCanExecute = true;
                    return;
                }
            }
                Config.MiniClient.ClientRequestResult -= MiniClient_ClientRequestResult;
            MessageBox.Show("找不到指定的好友！", Config.Name, MessageBoxButton.OK, MessageBoxImage.Warning);
            addFriendCanExecute = true;
        }

        private void AddFriendAction(RequestResult result, User user)
        {
            if (result.Success == true)
            {
                SeekFriend.SeekFriendWindow.Dispatcher.Invoke(() =>
                {
                    (Home.HomeWindow?.DataContext as HomeViewModel)?.LoadFriendCommand?.Execute(null);
                });
                Config.MiniClient.ClientRequestResult -= MiniClient_ClientRequestResult;
                MessageBox.Show($"好友 {user.NickName}({user.UserName}) 添加成功！", Config.Name, MessageBoxButton.OK, MessageBoxImage.Asterisk);
                addFriendCanExecute = true;
                return;
            }
            Config.MiniClient.ClientRequestResult -= MiniClient_ClientRequestResult;
            MessageBox.Show("添加好友失败！", Config.Name, MessageBoxButton.OK, MessageBoxImage.Warning);
            addFriendCanExecute = true;
        }

        private void MiniClient_ClientRequestResult(object sender, RequestResultEventArgs e)
        {
            _requestResultAction?.Invoke(e.Result);
        }
    }
}