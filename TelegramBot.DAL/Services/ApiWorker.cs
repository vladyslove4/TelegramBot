using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using TelegramBot.Domain.Interfaces;
using TelegramBot.Domain.Model;

namespace TelegramBot.DAL.Services;

public class ApiWorker : IApiWorker
{
    private HttpClient _httpClient;
    private readonly ILogger<BotEngine> _logger;
    AppConfigurationOptions _appConfigurationOptions;

    public List<ExchangeRate> exchangeRate { get; set; }
    public ApiWorker(HttpClient httpClient, ILogger<BotEngine> logger, AppConfigurationOptions appConfiguration)
    {
        _appConfigurationOptions = appConfiguration;
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<string> GetResponseAsync(string currency, DateTime dateTime)
    {

        string formatString = $"{_appConfigurationOptions.ApiUrl}{dateTime.ToString("dd.MM.yyyy")}";

        try
        {
            using HttpResponseMessage response = await _httpClient.GetAsync(formatString);

            if (response is not null)
            {
                var jsonResponse = await response.Content.ReadAsStringAsync();
                var root = JsonConvert.DeserializeObject<RootObject>(jsonResponse);
                var exchangeRates = root.exchangeRate;

                if (exchangeRates is not null)
                {
                    var result = GetCurrency(exchangeRates, currency);

                    return result;
                }
            }
        }

        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Exception handled", ex.Message);
        }

        return "Bad request";
    }

    private string GetCurrency(List<ExchangeRate> exchangeRates, string currency)
    {
        if (exchangeRates is null)
        {
            return "Doesnt have exchange rate";
        }

        var selectedRate = exchangeRates.FirstOrDefault(exchangeRate => exchangeRate.currency == currency);

        if (selectedRate is null)
        {
            return "Doesnt have currency for this date";
        }

        return $"Purchase: {selectedRate.purchaseRate} Sale: {selectedRate.saleRate}";
    }
}