using System.Collections.Generic;
using System.Threading.Tasks;
using QuoteBot.Filters;
using Trumpi.Twitch.Irc;

namespace QuoteBot
{
    public class CommandParser
    {
        public CommandParser(IEnumerable<IChatCommand> commands)
        {
            foreach (var command in commands)
            {
                _root.AddCommand(new PermissionFilter(command));
            }
        }
        
        private class Node
        {
            private IChatCommand _handler;
            private readonly Dictionary<char, Node> _nodes = new();

            public IChatCommand Scan(
                string input,
                int index)
            {
                if (index >= input.Length) return _handler;

                char next = input[index];

                if (_nodes.TryGetValue(next, out var child))
                {
                    return child.Scan(input, index + 1) ?? Accept(next);
                }

                return Accept(next);
            }

            public void AddCommand(
                IChatCommand command)
            {
                AddCommand(command.Command, command, 0);
            }

            private void AddCommand(
                string commandName,
                IChatCommand commandWithHandler,
                int index)
            {
                if (index > 0 && index == commandName.Length)
                {
                    _handler = commandWithHandler;
                    return;
                }

                var current = commandName[index];

                if (!_nodes.ContainsKey(current))
                {
                    _nodes.Add(current, new Node());
                }
                _nodes[current].AddCommand(commandName, commandWithHandler, index + 1);
            }

            private IChatCommand Accept(char next)
            {
                if (char.IsWhiteSpace(next))
                {
                    return _handler;
                }

                return null;
            }
        }

        private readonly Node _root = new();

        public async Task HandleAsync(ITwitchChatConnection chatConnection, string roomId, IrcMessage message)
        {
            var command = _root.Scan(message.Parameters[1]?.ToLowerInvariant(), 0);
            if (command == null) return;
            await command.HandleCommandAsync(chatConnection, roomId, message);
        }
    }
}