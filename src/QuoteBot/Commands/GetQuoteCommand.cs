using System;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Options;
using Npgsql;
using QuoteBot.Filters;
using Trumpi.Twitch.Irc;

namespace QuoteBot
{
    public class GetQuoteCommand : IChatCommand
    {
        private readonly IOptions<Config> _options;
        private DateTime _quoteCommandCooldownExpiry;
        private readonly Random rng = new();

        public GetQuoteCommand(IOptions<Config> options)
        {
            _options = options;
        }
        
        public string Command => "!quote";
        public PermissionLevel DefaultPermissions => PermissionLevel.Everyone;

        public async Task HandleCommandAsync(ITwitchChatConnection chatConnection, string roomId, IrcMessage message)
        {
            if (DateTime.UtcNow > _quoteCommandCooldownExpiry)
            {
                await ExecuteQuoteRandom(chatConnection);
                _quoteCommandCooldownExpiry = DateTime.UtcNow.AddSeconds(30);
            }
        }
        
        private async Task ExecuteQuoteRandom(ITwitchChatConnection chatConnection)
        {
            const string countSql = "SELECT COUNT(1) FROM Quotes";
            const string sql = @"
                SELECT q.Text FROM Quotes q
                ORDER BY q.id
                OFFSET @offset
                LIMIT 1
            ";

            await using var connection = new NpgsqlConnection(_options.Value.ConnectionString);

            var count = await connection.ExecuteScalarAsync<int>(countSql);
            var offset = rng.Next(0, count);
            var result = (await connection.QueryAsync(sql, new { offset })).ToList();
            var quote = result.FirstOrDefault()?.text;
            if (!string.IsNullOrEmpty(quote))
            {
                await chatConnection.WriteMessage(new IrcMessage(
                    "PRIVMSG",
                    $"#{_options.Value.Channel}",
                    quote)
                );
            }
            else
            {
                await chatConnection.WriteMessage(new IrcMessage(
                    "PRIVMSG",
                    $"#{_options.Value.Channel}",
                    "No quotes here")
                );
            }
        }
    }
}