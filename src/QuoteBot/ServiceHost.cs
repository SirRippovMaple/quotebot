using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Serilog;
using Trumpi.Twitch.Irc;

namespace QuoteBot
{
    public class ServiceHost : IHostedService, ITwitchChatStreamNotifications
    {
        private readonly ILogger _logger;
        private readonly IOptions<Config> _options;
        private readonly CommandParser _commandParser;
        private readonly TwitchChatConnection _connection;
        private string _roomId;

        public ServiceHost(ILogger logger, IOptions<Config> options, CommandParser commandParser)
        {
            _logger = logger;
            _options = options;
            _commandParser = commandParser;
            _connection = new TwitchChatConnection(this);
        }
        
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await _connection.ConnectAsync(new TwitchChatConnectionParameters(
                    "irc.chat.twitch.tv",
                    6697,
                    true,
                    _options.Value.User,
                    _options.Value.Password
                )
            );
            await _connection.Join(_options.Value.Channel);
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await _connection.DisconnectAsync();
            _connection.Dispose();
        }

        public async Task OnMessageAsync(IrcMessage message)
        {
            try
            {
                switch (message.Command)
                {
                    case "ROOMSTATE":
                        _roomId = message.Tags["room-id"];
                        break;
                    case "PRIVMSG":
                        await _commandParser.HandleAsync(_connection, _roomId, message);
                        break;
                }
            }
            catch (Exception e)
            {
                _logger.Error(e, "Error");
            }
        }

        public Task OnErrorAsync(Exception exception)
        {
            _logger.Error(exception, "Error");
            return Task.CompletedTask;
        }
    }
}