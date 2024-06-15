using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types;
using Balance.Platforms;
using Telegram.Bot.Types.ReplyMarkups;
using Google.Apis.Auth.OAuth2;
using Balance.GoogleSheet;
using Google.Apis.Sheets.v4.Data;

namespace Balance
{
    internal class Program
    {
        public static ITelegramBotClient? ClientBot { get; private set; }
        public static string? BotName { get; private set; }

        public const string UsernameRoot = "procesosnova";

        private static Platform? _platform;

        private const string googleSheetId = "1fHBh6zeoE2_LPx_b6WO1zNd98SI29n4OaKB3IU4cuA0";

        static async Task Main(string[] args)
        {
            var credential = GoogleAuth.Login();
            var manager = new GoogleSheetsManager(credential);

            PlatformCollection.OnSelected += (p) => {
                p.OnOut += P_OnOut;
                _platform = p;
            };
            
            PlatformCollection.PlatformAdd(Massmo.MainGlobal,() => new Massmo(googleSheetId, manager));
            PlatformCollection.PlatformAdd(Garantex.MainGlobal, () => new Garantex(googleSheetId, manager));
            PlatformCollection.PlatformAdd(Sentoke.MainGlobal, () => new Sentoke(googleSheetId, manager));

            await Console.Out.WriteLineAsync("start");
            ClientBot = new TelegramBotClient("7007363424:AAFNHUTw5FFQ9EyDzIkVH2uYVAYkdQDFQ8g");
            BotName = (await ClientBot.GetMeAsync()).Username;
            await Console.Out.WriteLineAsync("my name: " + BotName);

            ClientBot.StartReceiving(Update, Exception);

            await Task.Delay(Timeout.Infinite);
        }

        private static async void P_OnOut(ITelegramBotClient client, Chat chat,bool isSuccess)
        {
            if (isSuccess)
            {
                var message = await ClientBot?.SendTextMessageAsync(chat, "Сохраняем в Excel")!;

                await _platform?.SheetPostAsync()!;

                await client.EditMessageTextAsync(chat, message.MessageId, "Успешно сохранено!");

                _platform.Dispose();
                _platform = null;
            } else
            {
                _platform?.Dispose();
                _platform = null;
                await ClientBot?.SendTextMessageAsync(chat, "Операция отменена")!;
            }
        }

        private static Task Update(ITelegramBotClient client, Update update, CancellationToken token)
        {
            if (update.Message is { } message && message.Text is { } text)
            {
                    if (message.Chat.Username != UsernameRoot)
                        return Task.CompletedTask;

                    if (text == "/start" && _platform == null)
                    return client.SendTextMessageAsync(message.Chat, "Добрый день!", replyMarkup: new InlineKeyboardMarkup(PlatformCollection.GetMains()),
                        cancellationToken: token);
                else if(text == "/clear" && _platform != null)
                {
                    _platform.Dispose();
                    _platform = null;

                    return client.SendTextMessageAsync(message.Chat, "Операция отменена!", replyMarkup: new InlineKeyboardMarkup(PlatformCollection.GetMains()),
                        cancellationToken: token);
                }
            }

            return _platform?.UpdateTG(client, update, token) ?? PlatformCollection.UpdateRG(client, update, token);
        }

        private static async Task Exception(ITelegramBotClient client, Exception exception, CancellationToken token)
        {
            await Console.Out.WriteLineAsync(exception.Message);
        }
    }
}
