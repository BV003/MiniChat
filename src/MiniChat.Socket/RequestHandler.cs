using MiniChat.Transmitting;
namespace MiniChatSocket.Server
{
    public delegate Task<RequestResult> RequestHandler<TEventArgs>(object sender, TEventArgs e);
}
