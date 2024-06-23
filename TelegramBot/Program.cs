using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TelegramBot.DAL;
using TelegramBot.DAL.Services;
using TelegramBot.Domain.Services;

class Program
{
    static async Task Main()
    {
        var configuration = new ConfigurationBuilder()
             .AddJsonFile("appsettings.json")
             .AddEnvironmentVariables()
             .Build();

        var services = new ServiceCollection();
        
        services.AddSingleton<IConfiguration>(configuration);

        services.Configure<AppConfigurationOptions>(configuration.GetSection(AppConfigurationOptions.Configuration));

        services.AddSingleton<MessageHandler>();

        services.AddLogging(loggingBuilder =>
        {
            loggingBuilder.AddConsole();
        });
        
        services.AddDataAccessServices(configuration); 

        services.AddSingleton<BotEngine>();

        var serviceProvider = services.BuildServiceProvider();

        using var scope = serviceProvider.CreateScope();

        // Start
        var botEngine = scope.ServiceProvider.GetRequiredService<BotEngine>();
        await botEngine.ListenForMessagesAsync();

        Console.WriteLine("Press any key to exit...");
        Console.ReadKey();
    }
}