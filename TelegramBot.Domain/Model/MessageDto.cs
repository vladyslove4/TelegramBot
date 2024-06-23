namespace TelegramBot.Domain.Model;

public class MessageDto
{
    public long userId { get; set; }
    public long chatId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
}