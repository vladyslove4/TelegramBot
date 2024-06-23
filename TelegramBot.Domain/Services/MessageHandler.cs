using System.Globalization;
using System.Text;
using TelegramBot.Domain.Interfaces;
using TelegramBot.Domain.Model;

namespace TelegramBot.Domain.Services;

public class MessageHandler : IMessageHandler
{
    private readonly IApiWorker _apiWorker;

    private List<string> currencyCode;

    private readonly Dictionary<long, Client> userDictionary;

    public MessageHandler(IApiWorker apiWorker)
    {
        _apiWorker = apiWorker;

        userDictionary = new Dictionary<long, Client>();
        currencyCode = new List<string> { "PLN", "CHF", "GBP", "USD", "EUR" };
    }

    public async Task<string> ProcessMessage(MessageDto message, CancellationToken cancellationToken)
    {
        var client = GetUser(message, cancellationToken);

        switch (client.userState)
        {
            case UserState.Start:
                return ProcessStartState(message.Text, client);

            case UserState.AwaitingCurrency:
                return ProcessAwaitingCurrencyState(message.Text, client);

            case UserState.AwaitingDate:
                return await ProcessAwaitingDateState(message.Text, client, cancellationToken);

            default:
                return SendDefaultMessage();
        }
    }

    private string ProcessStartState(string message, Client client)
    {
        if (CheckStartMessage(message))
        {
            return SendAwaitingCurrencyMessage(client);
        }
        else
        {
            return SendWelcomeMessage(client.UserName);
        }
    }

    private string ProcessAwaitingCurrencyState(string message, Client client)
    {
        if (TryParseCurrencyMethod(message, client))
        {
            return SendAwaitingDateMessage(client);
        }
        else
        {
            return SendWrongCodeMessage();
        }
    }

    private async Task<string> ProcessAwaitingDateState(string message, Client client, CancellationToken cancellationToken)
    {
        string dateFormat = "dd.MM.yyyy"; 

        if (DateTime.TryParseExact(message, dateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime date))
        {
            client.dateTime = date;
            return await SendResultMessage(client, cancellationToken);
        }
        else
        {
            return SendWrongDateMessage();
        }
    }

    public bool TryParseCurrencyMethod(string text, Client client)
    {
        var formatedText = text.TrimStart('/').ToUpper();

        if (!currencyCode.Contains(formatedText))

            return false;

        client.Currency = formatedText;

        return true;
    }

    public Client GetUser(MessageDto message, CancellationToken cancellationToken)
    {
        var userId = message.chatId;
        var name = message.Name;

        if (userDictionary.ContainsKey(userId))
        {
            var client = userDictionary[userId];

            return client;
        }
        else
        {
            var newClient = new Client { UserId = userId, UserName = name, userState = UserState.Start };
            userDictionary.Add(userId, newClient);

            return newClient;
        }
    }

    private string FormatCurrencyCodes(List<string> currencyCodes)
    {
        var formattedString = new StringBuilder();

        foreach (var code in currencyCodes)
        {
            formattedString.Append($"/{code} ");
        }

        return formattedString.ToString().Trim();
    }

    private bool CheckStartMessage(string text)
    {
        var cleanText = text.TrimStart('/').ToLower();

        return cleanText == "start";
    }

    private string SendWrongCodeMessage()
    {
        var allCurrency = FormatCurrencyCodes(currencyCode);
        string welcomeText = $"Wrong code Input,  We have \n {allCurrency}";

        return welcomeText;
    }

    private string SendWrongDateMessage()
    {
        string welcomeText = $"Wrong date Input,  enter the date in dd-mm-yyyy format";

        return welcomeText;
    }

    private string SendWelcomeMessage(string username)
    {
        string welcomeText = $"Welcome! {username}. Type /start for assistance.";

        return welcomeText;
    }

    private string SendAwaitingCurrencyMessage(Client client)
    {
        string currencyText = $"{client.UserName}, send me currency code for example /USD.";
        client.userState = UserState.AwaitingCurrency;

        return currencyText;
    }

    private string SendAwaitingDateMessage(Client client)
    {
        client.userState = UserState.AwaitingDate;

        return $"{client.UserName}, send me date fo example 20.12.2020 .";
    }

    private async Task<string> SendResultMessage(Client client, CancellationToken cancellationToken)
    {
        var response = await _apiWorker.GetResponseAsync(client.Currency, client.dateTime);
        var result = $"{client.Currency} to UAH exchange rate on {client.dateTime.ToString("dd.MM.yyyy")} \n  {response}.\n" +
            $"if you wanna continue type /start again";

        client.userState = UserState.Start;

        return result;
    }

    private string SendDefaultMessage()
    {
        string responseText = "I don't understand that command. Type hello or exchange for assistance.";

        return responseText;
    }
}