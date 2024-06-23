using AutoMapper;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TelegramBot.Domain.Model;
using TelegramBot.Domain.Services;

namespace TelegramBot.DAL.Services;

public class BotEngine
{
    private readonly ITelegramBotClient _botClient;
    private readonly ILogger<BotEngine> _logger;
    private readonly IMapper _mapper;
    private readonly MessageHandler _messageHandler;

    public BotEngine(ITelegramBotClient botClient, ILogger<BotEngine> logger, MessageHandler messageHandler, IMapper mapper)
    {
        _botClient = botClient;
        _logger = logger;
        _messageHandler = messageHandler;
        _mapper = mapper;
    }

    public async Task ListenForMessagesAsync()
    {
        using var cts = new CancellationTokenSource();

        var receiverOptions = new ReceiverOptions
        {
            AllowedUpdates = Array.Empty<UpdateType>() // receive all update types
        };

        _botClient.StartReceiving(
            updateHandler: HandleUpdateAsync,
            pollingErrorHandler: HandlePollingErrorAsync,
            receiverOptions: receiverOptions,
            cancellationToken: cts.Token
        );

        var me = await _botClient.GetMeAsync();

        _logger.LogInformation($"Start listening for @{me.Username}");
    }

    public async Task HandleUpdateAsync(ITelegramBotClient _, Update update, CancellationToken cancellationToken)
    {
        if (update.Message is not { } message)
        {
            return;
        }

        _logger.LogInformation("Received a message from {UserId}", message.From.Id);

        var answer = await _messageHandler.ProcessMessage(_mapper.Map<MessageDto>(message), cancellationToken);

        Message sentMessage = await _botClient.SendTextMessageAsync(
        chatId: message.Chat.Id,
        text: answer,
        cancellationToken: cancellationToken);
    }

    public Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
        _logger.LogError("An error occurred while polling for updates: {ErrorMessage}", exception.Message);
        return Task.CompletedTask;
    }
}