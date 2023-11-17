using RoboWorkerService.Interfaces;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace RoboWorkerService.Telegram;

public interface ITelegram
{
    Task SendOkTextAsync(string text);
    Task SendErrorTextAsync(string text);
    Task SendErrorTextAsync(Exception ex, string text);
    public event TelegramAPi.TelegramEvenHandler TelegramSays;
}

public class Telegram : TelegramAPi, ITelegram
{
    private readonly IAppRobo _appRobo;
    private readonly ILogger<Telegram> _logger;

    public Telegram(IAppRobo appRobo, ILogger<Telegram> logger) : base(appRobo.Config.TelegramToken, appRobo.GetAppToken())
    {
        _appRobo = appRobo;
        _logger = logger;

        //   var me = BotClient.GetMeAsync().Result;
    }

    public async Task SendOkTextAsync(string text)
    {
        if (ChatId == 0)
        {
            _logger.LogInformation(_appRobo.Config.ComplileEnvironment + ": " + text);
            return;
        }

        _logger.LogInformation(text);
        Message message = await BotClient.SendTextMessageAsync(
            chatId: ChatId,
            text: text,
            cancellationToken: _appRobo.GetAppToken());
    }

    public Task SendErrorTextAsync(Exception ex, string text)
    {
        return SendErrorTextAsync(_appRobo.Config.ComplileEnvironment + ": " + text + Environment.NewLine + ex.ToString());
    }

    public Func<string, string> TelegramSay { get; set; }

    public async Task SendErrorTextAsync(string text)
    {
        if (ChatId == 0)
        {
            _logger.LogError(text);
            return;
        }

        _logger.LogError(text);
        Message message = await BotClient.SendTextMessageAsync(
            chatId: ChatId,
            text: text,
            cancellationToken: _appRobo.GetAppToken());
    }
}