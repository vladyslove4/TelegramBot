using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Telegram.Bot;
using TelegramBot.DAL;
using TelegramBot.DAL.Services;
using TelegramBot.Domain.Interfaces;

public static class DataAccessServiceExtension
{
    
    public static IServiceCollection AddDataAccessServices(this IServiceCollection services, IConfiguration configuration)
    {

        var appConfig = new AppConfigurationOptions();
        configuration.GetSection(AppConfigurationOptions.Configuration).Bind(appConfig);

        var botClient = new TelegramBotClient(appConfig.BotToken);

        services.AddSingleton<ITelegramBotClient>(botClient);
        services.AddSingleton(appConfig);

        services.AddHttpClient<IApiWorker, ApiWorker>();

        services.AddSingleton<ApiWorker>();

        services.AddScoped<BotEngine>();

        services.AddAutoMapper(typeof(MappingProfile));

        return services;
    }
}