using TelegramBot.Domain.Model;

namespace TelegramBot.Domain.Interfaces
{
    public interface IApiWorker
    {
        Task<string> GetResponseAsync(string currency, DateTime dateTime);
    }
}