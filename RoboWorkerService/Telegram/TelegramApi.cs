using System.Text.RegularExpressions;
using Serilog;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;


public class TelegramAPi
{
    protected TelegramBotClient BotClient = null!;
    protected long ChatId;

    public delegate string TelegramEvenHandler(string word);

    public event TelegramEvenHandler TelegramSays;

    protected TelegramAPi(string token, CancellationToken cancellationToken)
    {
        try
        {
            Register(token, cancellationToken).Wait();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "telegram Api failed!");
        }
    }

    private async Task Register(string token, CancellationToken cancellationToken)
    {
// StartReceiving does not block the caller thread. Receiving is done on the ThreadPool.
        var receiverOptions = new ReceiverOptions
        {
            AllowedUpdates = Array.Empty<UpdateType>() // receive all update types
        };
        BotClient = new TelegramBotClient(new TelegramBotClientOptions(token));
        BotClient.StartReceiving(
            updateHandler: HandleUpdateAsync,
            pollingErrorHandler: HandlePollingErrorAsync,
            receiverOptions: receiverOptions,
            cancellationToken: cancellationToken
        );

        var me = await BotClient.GetMeAsync();
        Console.WriteLine($"Start listening for @{me.Username}");

        async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            // Only process Message updates: https://core.telegram.org/bots/api#message
            if (update.Message is not { } message)
                return;
            // Only process text messages
            if (message.Text is not { } messageText)
                return;

            ChatId = message.Chat.Id;

            string pattern = @"^/\b[a-zA-Z0-9_]+\b$";

            Match m = Regex.Match(message.Text, pattern, RegexOptions.IgnoreCase);

            if (!m.Success)
            {
                return;
            }

            if (TelegramSays is null) return;

            var msg = TelegramSays?.Invoke(m.Value);

            Console.WriteLine($"Received a '{messageText}' message in chat {ChatId}.");
            // Echo received message text
            Message sentMessage = await botClient.SendTextMessageAsync(
                chatId: ChatId,
                text: msg,
                cancellationToken: cancellationToken);
        }

        Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            var ErrorMessage = exception switch
            {
                ApiRequestException apiRequestException
                    => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                _ => exception.ToString()
            };

            Console.WriteLine(ErrorMessage);
            return Task.CompletedTask;
        }
    }
}