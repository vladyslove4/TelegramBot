using Moq;
using TelegramBot.Domain.Interfaces;
using TelegramBot.Domain.Model;
using TelegramBot.Domain.Services;

namespace TelegramBot.Tests.TelegramBot.Domain.Services.Tests;

public class MessageHandlerTests
{
    private readonly Mock<IApiWorker> apiWorkerMock;
    private readonly MessageHandler messageHandler;
    private readonly CancellationToken cancellationToken;

    public MessageHandlerTests()
    {
        apiWorkerMock = new Mock<IApiWorker>();
        messageHandler = new MessageHandler(apiWorkerMock.Object);
        cancellationToken = new CancellationToken();
    }

    [Fact]
    public async Task ProcessMessage_PositiveAnswer_StartStatwChange()
    {
        // Arrange
        var messageDto = new MessageDto { userId = 1, Name = "TestUser", Text = "/start" };

        // Act
        var result = await messageHandler.ProcessMessage(messageDto, cancellationToken);

        // Assert
        Assert.Contains("TestUser, send me currency code for example /USD", result);
    }

    [Fact]
    public async Task ProcessMessage_NegativeAnswer_StartStateDoesnChange()
    {
        // Arrange
        var messageDto = new MessageDto { userId = 1, Name = "TestUser", Text = "USD" };

        // Act
        var result = await messageHandler.ProcessMessage(messageDto, cancellationToken);

        // Assert
        Assert.Contains("Welcome! TestUser. Type /start for assistance", result);
    }

    [Fact]
    public async Task ProcessMessage_PositiveAnswer_AwaitingCurrencyStateChange()
    {
        // Arrange
        var firstMessageDto = new MessageDto { userId = 2, Name = "TestUser", Text = "start" };
        var SecondMessageDto = new MessageDto { userId = 2, Name = "TestUser", Text = "USD" };

        // Act
        var firstResult = await messageHandler.ProcessMessage(firstMessageDto, cancellationToken);
        var secondResult = await messageHandler.ProcessMessage(SecondMessageDto, cancellationToken);

        // Assert
        Assert.Contains("TestUser, send me date fo example 20.12.2020", secondResult);
    }

    [Fact]
    public async Task ProcessMessage_NegativeAnswer_AwaitingCurrencyStateDoesntChange()
    {
        // Arrange
        var firstMessageDto = new MessageDto { userId = 2, Name = "TestUser", Text = "start" };
        var SecondMessageDto = new MessageDto { userId = 2, Name = "TestUser", Text = "USD1" };

        // Act
        var firstResult = await messageHandler.ProcessMessage(firstMessageDto, cancellationToken);
        var secondResult = await messageHandler.ProcessMessage(SecondMessageDto, cancellationToken);

        // Assert
        Assert.Contains("Wrong code Input,  We have \n /PLN /CHF /GBP /USD /EUR", secondResult);
    }

    [Fact]
    public void TryParseCurrencyMethod_ValidInput_ShouldReturnTrueAndSetCurrency()
    {
        // Arrange

        var client = new Client();
        var inputText = "USD";

        // Act
        var result = messageHandler.TryParseCurrencyMethod(inputText, client);

        // Assert
        Assert.True(result);
        Assert.Equal("USD", client.Currency);
    }

    [Fact]
    public void TryParseCurrencyMethod_InvalidInput_ShouldReturnFalseAndNotSetCurrency()
    {
        // Arrange
        var client = new Client();
        var inputText = "something";

        // Act
        var result = messageHandler.TryParseCurrencyMethod(inputText, client);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void GetUser_UserExists_ReturnsExistingClient()
    {
        // Arrange
        var message = new MessageDto { chatId = 2, Name = "test2" };

        var userDictionary = new Dictionary<int, Client>

    {
    { 1, new Client { UserId = 1, UserName = "test", userState = UserState.Start } },
    { 2, new Client { UserId = 2, UserName = "test2", userState = UserState.Start } }
    };

        // Act
        var result = messageHandler.GetUser(message, cancellationToken);


        // Assert
        Assert.Equal(result.UserName, message.Name);
        
    }

    [Fact]
    public void GetUser_UserDoesNotExist_AddsNewClientToDictionary()
    {
        // Arrange
        var message = new MessageDto { chatId = 1, Name = "test" };
        var userDictionary = new Dictionary<int, Client>();

        // Act
        var result = messageHandler.GetUser(message, cancellationToken);

        // Assert
        
        Assert.True(result.UserId == 1);
        
    }
}