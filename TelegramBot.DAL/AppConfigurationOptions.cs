namespace TelegramBot.DAL
{
    public class AppConfigurationOptions
    {
        public const string Configuration = "Configuration";

        public string BotToken { get; set; } = String.Empty;
        public string ApiUrl { get; set; } = String.Empty;
    }
}
