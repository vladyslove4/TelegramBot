namespace TelegramBot.Domain.Model;

public class ExchangeRate
{
    public string? baseCurrency { get; set; }
    public string? currency { get; set; }
    public double? saleRateNB { get; set; }
    public double? purchaseRateNB { get; set; }
    public double? saleRate { get; set; }
    public double? purchaseRate { get; set; }
}
public class RootObject
{
    public List<ExchangeRate> exchangeRate { get; set; }
}