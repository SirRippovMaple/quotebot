using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace QuoteBot
{
    class Program 
    {

        static async Task Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .CreateLogger();

            var host = new HostBuilder()
                .ConfigureAppConfiguration(cb =>
                {
                    cb.AddEnvironmentVariables("qb_");
                })
                .ConfigureServices(ConfigureServices);
            await host.RunConsoleAsync();
        }

        private static void ConfigureServices(HostBuilderContext context, IServiceCollection collection)
        {
            collection.AddHostedService<ServiceHost>();
            collection.AddOptions<Config>()
                .Bind(context.Configuration);
            collection.AddSingleton(_ => Log.Logger);
            collection.AddSingleton<IChatCommand, AddQuoteCommand>();
            collection.AddSingleton<IChatCommand, GetQuoteCommand>();
            collection.AddSingleton<IChatCommand, DeleteQuoteCommand>();
            collection.AddSingleton<CommandParser>();
        }
    }
}