using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;
using TelegramBot.DAL;
using TelegramBot.DAL.Services;
using TelegramBot.Domain.Model;

namespace TelegramBot.Tests.TelegramBot.API.Services.Tests;

public class ApiWorkerTests
{
    [Fact]
    public async Task GetResponseAsync_ValidResponse_ReturnsCurrencyInfo()
    {
        // Arrange
        var expectedCurrency = "USD";
        DateTime expectedDate = DateTime.Now;
        var exchangeRates = new List<ExchangeRate>
    {
        new ExchangeRate { currency = "USD", purchaseRate = 1.2, saleRate = 1.3 },
        new ExchangeRate { currency = "EUR", purchaseRate = 0.8, saleRate = 0.9 }

    };

        var rootObject = new RootObject { exchangeRate = exchangeRates };
        var serializedResponse = JsonConvert.SerializeObject(rootObject);

        var mockHandler = new Mock<HttpMessageHandler>();
        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                Content = new StringContent(serializedResponse)
            });

        var httpClient = new HttpClient(mockHandler.Object);
        var logger = new Mock<ILogger<BotEngine>>();
        var appConfigurationOptions = new AppConfigurationOptions { ApiUrl = "http://test/" };
        var apiWorker = new ApiWorker(httpClient, logger.Object, appConfigurationOptions);

        // Act
        var result = await apiWorker.GetResponseAsync(expectedCurrency, expectedDate);

        // Assert
        Assert.Equal("Purchase: 1,2 Sale: 1,3", result);
    }

    [Fact]
    public async Task GetResponseAsync_NotValidResponse_ReturnsDefaultResponse()
    {
        // Arrange
        var expectedCurrency = "USD";
        DateTime expectedDate = new DateTime(2024, 12, 2);
        var exchangeRates = new List<ExchangeRate>();
    
        var rootObject = new RootObject { exchangeRate = exchangeRates };
        var serializedResponse = "{\"exchangeRate\":[]}";

        var mockHandler = new Mock<HttpMessageHandler>();
        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                Content = new StringContent(serializedResponse)
            });

        var httpClient = new HttpClient(mockHandler.Object);
        var logger = new Mock<ILogger<BotEngine>>();
        var appConfigurationOptions = new AppConfigurationOptions { ApiUrl = "http://test/" };
        var apiWorker = new ApiWorker(httpClient, logger.Object, appConfigurationOptions);

        // Act
        var result = await apiWorker.GetResponseAsync(expectedCurrency, expectedDate);

        // Assert
        Assert.Equal("Doesnt have currency for this date", result);
    }
}