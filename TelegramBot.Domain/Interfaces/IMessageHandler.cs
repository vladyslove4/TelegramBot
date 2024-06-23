using TelegramBot.Domain.Model;

namespace TelegramBot.Domain.Interfaces;

public interface IMessageHandler
{
    Task<string> ProcessMessage(MessageDto message, CancellationToken cancellationToken);
}