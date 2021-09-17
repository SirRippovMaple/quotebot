using System;
using System.Threading.Tasks;
using Trumpi.Twitch.Irc;

namespace QuoteBot.Filters
{
    [Flags]
    public enum PermissionLevel
    {
        Nobody = 0,
        Everyone,
        Subscribers,
        Moderators,
        Vips,
        Broadcaster
    }
    
    public class PermissionFilter : IChatCommand
    {
        private readonly IChatCommand _innerCommand;

        public PermissionFilter(IChatCommand innerCommand)
        {
            _innerCommand = innerCommand;
        }

        public string Command => _innerCommand.Command;
        public PermissionLevel DefaultPermissions => _innerCommand.DefaultPermissions;

        public Task HandleCommandAsync(ITwitchChatConnection chatConnection, string roomId, IrcMessage message)
        {
            if (HasPermission(message, roomId))
            {
                return _innerCommand.HandleCommandAsync(chatConnection, roomId, message);
            }
            return Task.CompletedTask;
        }

        private bool HasPermission(IrcMessage message, string roomId)
        {
            if (DefaultPermissions.HasFlag(PermissionLevel.Everyone))
            {
                return true;
            }

            if (DefaultPermissions.HasFlag(PermissionLevel.Moderators))
            {
                var isModerator = message.Tags["mod"] == "1" || message.Tags["user-id"] == roomId;
                return isModerator;
            }

            if (DefaultPermissions.HasFlag(PermissionLevel.Broadcaster))
            {
                var isBroadcaster = message.Tags["user-id"] == roomId;
                return isBroadcaster;
            }

            return false;
        }
    }
}