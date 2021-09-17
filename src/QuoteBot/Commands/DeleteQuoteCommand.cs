using System;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Options;
using Npgsql;
using QuoteBot.Filters;
using Trumpi.Twitch.Irc;

namespace QuoteBot
{
    public class DeleteQuoteCommand : IChatCommand
    {
        private readonly IOptions<Config> _configuration;
        public string Command => "!delquote";
        public PermissionLevel DefaultPermissions => PermissionLevel.Moderators;

        public DeleteQuoteCommand(IOptions<Config> configuration)
        {
            _configuration = configuration;
        }
        
        public async Task HandleCommandAsync(ITwitchChatConnection chatConnection, string roomId, IrcMessage message)
        {
            const string sql = @"
                DELETE FROM Quotes q
                WHERE id = @id
            ";
            
            var parameterParser = new ParameterParser(message, Command);
            var quoteNumber = parameterParser.ReadInt();
            
            await using var connection = new NpgsqlConnection(_configuration.Value.ConnectionString);

            await connection.ExecuteAsync(sql, new { id = quoteNumber });
            
            await chatConnection.WriteMessage(new IrcMessage(
                "PRIVMSG",
                $"#{_configuration.Value.Channel}",
                $"Deleted quote {quoteNumber}")
            );
        }
    }
}