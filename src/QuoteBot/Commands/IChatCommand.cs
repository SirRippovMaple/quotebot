using System.Threading.Tasks;
using QuoteBot.Filters;
using Trumpi.Twitch.Irc;

namespace QuoteBot
{
    public interface IChatCommand
    {
        string Command { get; }
        PermissionLevel DefaultPermissions { get; }
        Task HandleCommandAsync(ITwitchChatConnection chatConnection, string roomId, IrcMessage message);
    }
}