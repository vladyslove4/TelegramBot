namespace TelegramBot.Domain.Model;

public class Client
{
    public long UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Currency { get; set; } = string.Empty;
    public DateTime dateTime { get; set; }

    public UserState userState;
}

public enum UserState
{
    Start,
    AwaitingCurrency,
    AwaitingDate,
    Completed
}