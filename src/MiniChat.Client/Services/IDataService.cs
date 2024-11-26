using MiniChat.Client.Models;
using MiniChat.Transmitting;
namespace MiniChat.Client.Services
{
    public interface IDataService
    {
        UserModel GetUserModel(User user);
    }
}