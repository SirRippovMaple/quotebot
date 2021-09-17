using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Options;
using Npgsql;
using QuoteBot.Filters;
using Trumpi.Twitch.Irc;

namespace QuoteBot
{
    public class AddQuoteCommand : IChatCommand
    {
        private readonly IOptions<Config> _options;

        public AddQuoteCommand(IOptions<Config> options)
        {
            _options = options;
        }
        
        public string Command => "!addquote";
        public PermissionLevel DefaultPermissions => PermissionLevel.Moderators;

        public async Task HandleCommandAsync(ITwitchChatConnection chatConnection, string roomId, IrcMessage message)
        {
            const string sql = @"
                INSERT INTO Quotes (Text, CreatedByTwitchId)
                VALUES(@quote, @createdByTwitchId)
                RETURNING Id;
            ";
            
            var messageText = message.Parameters[1];
            var createdByTwitchId = message.Tags["user-id"];
            var quote = messageText.Substring(Command.Length + 1);
            
            await using var connection = new NpgsqlConnection(_options.Value.ConnectionString);

            var quoteId = await connection.ExecuteScalarAsync<int>(sql, new { quote, createdByTwitchId });

            if (quoteId > 0)
            {
                await chatConnection.WriteMessage(new IrcMessage(
                    "PRIVMSG",
                    $"#{_options.Value.Channel}",
                    $"Added quote number {quoteId}.")
                );
            }
            else
            {
                await chatConnection.WriteMessage(new IrcMessage(
                    "PRIVMSG",
                    $"#{_options.Value.Channel}",
                    $"Error adding quote.")
                );
            }
        }
    }
}