using Telegram.Bot;
using Telegram.Bot.Types;
using RED_WHITE_TG_BOT.Core;
using RED_WHITE_TG_BOT.BotCore;

namespace RED_WHITE_TG_BOT
{
    public static class Program
    {
        public static ITelegramBotClient? ClientBot { get; set; }
        private static CommandCallback[]? _callbacks;
        private static CommandCallback[]? _callbacksAdmin;
        private const long ADMIN_ID = 1735628011;

        static async Task Main(string[] args)
        {
            await Console.Out.WriteLineAsync("start");
            ClientBot = new TelegramBotClient("7100286967:AAEZV17IDYfxcTwa28oLrc1MqXldb0VsDb4");
            await Console.Out.WriteLineAsync("my name: " + (await ClientBot.GetMeAsync()).Username);

            await Console.Out.WriteLineAsync("Загружаем команды");

            _callbacks = [
               new(new()
                {
                    Command = "start",
                    Description = "Начать копить бонусы"
                }, BotEvents.StartCommandAsync),
                new(new()
                {
                    Command = "profile",
                    Description = "Профиль"
                },BotEvents.ProfileCommandAsync),
                new(new()
                {
                    Command = "points",
                    Description = "Получить ежедневные бонусы"
                }, BotEvents.PointsCommandAsync),
                new(new()
                {
                    Command = "bot",
                    Description = "Откуда бот?"
                }, BotEvents.BotAboutCommandAsync),
                new(new()
                {
                    Command = "help",
                    Description = "помощь"
                },
                (b,m) => b.SendTextMessageAsync(m.Chat.Id, string.Join('\n',_callbacks!.Select(o => $"/{o.BotCommand.Command} - {o.BotCommand.Description}"))
                    + '\n' + (m.Chat.Id == ADMIN_ID ? string.Join('\n', _callbacksAdmin!.Select(o => $"/{o.BotCommand.Command} - {o.BotCommand.Description}")) : string.Empty)))
            
            ];

            _callbacksAdmin = [
                new(new(){
                    Command = "all",
                    Description = "Получить список всех участников"
                }, BotEvents.ListUsersCommandAsync),
                new(new(){
                    Command = "clear",
                    Description = "Обнулить баллы"
                }, BotEvents.ClearCommandAsync)
                ];

            await ClientBot.SetMyCommandsAsync(_callbacks.Select(o => o.BotCommand));
            await Console.Out.WriteLineAsync("Успешно загрузили команды");

            ClientBot.StartReceiving(Update, Exception);

            await Task.Delay(Timeout.Infinite);
        }


        private static Task Update(ITelegramBotClient client, Update update, CancellationToken token)
        {
            if (update.Message is not { } message)
                return Task.CompletedTask;

            if (message.Text is not { } messageText)
                return Task.CompletedTask;

            Console.WriteLine($"username: {message.Chat.Username}, chatId: {message.Chat.Id}, message: {messageText}");

            for (int i = 0; i < _callbacks?.Length; i++)
                if (messageText.Contains(_callbacks[i].BotCommand.Command))
                    return _callbacks[i].InvokeAsync(client, message);

            if(message.Chat.Id == ADMIN_ID)
                for (int i = 0; i < _callbacksAdmin?.Length; i++)
                    if (messageText.Contains(_callbacksAdmin[i].BotCommand.Command))
                        return _callbacksAdmin[i].InvokeAsync(client, message);

            return BotEvents.DefaultCommandAsync(client, message);
        }

        private static async Task Exception(ITelegramBotClient client, Exception exception, CancellationToken token)
        {
            await Console.Out.WriteLineAsync(exception.Message);
        }
    }
}
