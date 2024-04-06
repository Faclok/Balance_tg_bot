using Telegram.Bot;
using Telegram.Bot.Types;
using RED_WHITE_TG_BOT.Core;
using RED_WHITE_TG_BOT.BotCore;
using Telegram.Bot.Types.Enums;

namespace RED_WHITE_TG_BOT
{
    public static class Program
    {
        public static ITelegramBotClient? ClientBot { get; set; }
        private static CommandTelegram[]? _callbacks;
        private static CommandTelegram[]? _callbacksAdmin;
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
                    + '\n' + (m.Chat.Id == ADMIN_ID ? string.Join('\n', _callbacksAdmin!.Select(o => $"/{o.BotCommand.Command} - {o.BotCommand.Description}")) : string.Empty), replyMarkup: CallbackButtonsTelegram.BackToMenu))

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
            switch (update.Type)
            {
                case UpdateType.Unknown:
                    break;
                case UpdateType.Message:
                    return UpdateMessage(client, update, token);

                case UpdateType.InlineQuery:
                    break;
                case UpdateType.ChosenInlineResult:
                    break;
                case UpdateType.CallbackQuery:
                    var messageAnswer = UpdateCallback(client, update, token);

                    if (update.CallbackQuery?.Message is { } message)
                        client.DeleteMessageAsync(message.Chat, message.MessageId, token);

                    return messageAnswer;

                case UpdateType.EditedMessage:
                    break;
                case UpdateType.ChannelPost:
                    break;
                case UpdateType.EditedChannelPost:
                    break;
                case UpdateType.ShippingQuery:
                    break;
                case UpdateType.PreCheckoutQuery:
                    break;
                case UpdateType.Poll:
                    break;
                case UpdateType.PollAnswer:
                    break;
                case UpdateType.MyChatMember:
                    break;
                case UpdateType.ChatMember:
                    break;
                case UpdateType.ChatJoinRequest:
                    break;
            }

            return Task.CompletedTask;
        }

        private static async Task<bool> CheckLogin(ITelegramBotClient client, User user, CancellationToken token)
        {
            var chat = await client.GetChatMemberAsync("@SelfDevelopment_X", user.Id, token);

            return chat.Status == ChatMemberStatus.Creator || chat.Status == ChatMemberStatus.Administrator || chat.Status == ChatMemberStatus.Member;
        }

        private static async Task UpdateMessage(ITelegramBotClient client, Update update, CancellationToken token)
        {

            if (update.Message is not { } message)
                return;

            if (message.Text is not { } messageText)
                return;

            Console.WriteLine($"username: {message.Chat.Username}, chatId: {message.Chat.Id}, message: {messageText}");

            if (message.From is { } from && !await CheckLogin(client, from, token))
            {
                await BotEvents.NoLoginAsync(client, message);
                return;
            }

            for (int i = 0; i < _callbacks?.Length; i++)
                if (messageText.Contains(_callbacks[i].BotCommand.Command))
                {
                    await _callbacks[i].InvokeAsync(client, message);
                    return;
                }

            if (message.Chat.Id == ADMIN_ID)
                for (int i = 0; i < _callbacksAdmin?.Length; i++)
                    if (messageText.Contains(_callbacksAdmin[i].BotCommand.Command))
                    {
                        await _callbacksAdmin[i].InvokeAsync(client, message);
                        return;
                    }

            await BotEvents.DefaultCommandAsync(client, message);
        }

        private static async Task UpdateCallback(ITelegramBotClient client, Update update, CancellationToken token)
        {

            if (update.CallbackQuery is not { } callbackQuery)
                return;

            if (callbackQuery.Data is not { } callbackQueryData)
                return;

            if (callbackQuery.Message is not { } message)
                return;

            Console.WriteLine($"username: {message.Chat.Username}, chatId: {message.Chat.Id}, callbackData: {callbackQueryData}");

            if (callbackQuery.From is { } from && !await CheckLogin(client, from, token))
            {
                await BotEvents.NoLoginAsync(client, message);
                return;
            }

            if (callbackQueryData == "menu")
            {
                await BotEvents.MenuCommandAsync(client, message);
                return;
            }

            for (int i = 0; i < _callbacks?.Length; i++)
                if (callbackQueryData.Contains(_callbacks[i].BotCommand.Command))
                {
                    await _callbacks[i].InvokeAsync(client, message);
                    return;
                }

            if (message.Chat.Id == ADMIN_ID)
                for (int i = 0; i < _callbacksAdmin?.Length; i++)
                    if (callbackQueryData.Contains(_callbacksAdmin[i].BotCommand.Command))
                    {
                        await _callbacksAdmin[i].InvokeAsync(client, message);
                        return;
                    }

            if(callbackQueryData == "checkLogin")
            {
                await BotEvents.MenuCommandAsync(client, message);
                return;
            }

            await BotEvents.DefaultCommandAsync(client, message);
        }

        private static async Task Exception(ITelegramBotClient client, Exception exception, CancellationToken token)
        {
            await Console.Out.WriteLineAsync(exception.Message);
        }
    }
}
