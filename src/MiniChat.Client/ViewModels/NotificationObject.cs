using System.ComponentModel;

namespace MiniChat.Client.ViewModels
{
    /// <summary>
    /// 通知对象
    /// </summary>
    public class NotificationObject : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}