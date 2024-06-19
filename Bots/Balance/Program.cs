using Telegram.Bot;
using Telegram.Bot.Types;
using Balance.Platforms;
using Telegram.Bot.Types.ReplyMarkups;
using Balance.GoogleSheet;
using System.Globalization;
using Microsoft.Extensions.Configuration;

namespace Balance
{
    internal class Program
    {
        public static ITelegramBotClient? ClientBot { get; private set; }
        public static string? BotName { get; private set; }

        public static string? UsernameRoot { get; private set; } = string.Empty;

        private static string? googleSheetId = string.Empty;

        private static Platform? _platform;

        static async Task Main(string[] args)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("config.json", optional: false);

            IConfiguration config = builder.Build();

            UsernameRoot = config.GetSection("usernameRoot").Get<string>();
            googleSheetId = config.GetSection("googleSheetId").Get<string>();

            var apiBot = config.GetSection("apiBot").Get<string>();

            if(apiBot == null)
            {
                Console.WriteLine($"no found property config: {nameof(apiBot)}");
                return;
            }

            if (googleSheetId == null)
            {
                Console.WriteLine($"no found property config: {nameof(googleSheetId)}");
                return;
            }

            if (UsernameRoot == null)
            {
                Console.WriteLine($"no found property config: {nameof(UsernameRoot)}");
                return;
            }

            await Console.Out.WriteLineAsync(UsernameRoot);

            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("ru-RU");
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
            ClientBot = new TelegramBotClient(apiBot);
            BotName = (await ClientBot.GetMeAsync()).Username;
            await Console.Out.WriteLineAsync("my name: " + BotName);

            ClientBot.StartReceiving(Update, Exception);

            await Task.Delay(Timeout.Infinite);
        }

        private static async void P_OnOut(Platform platform, ITelegramBotClient client, Chat chat,bool isSuccess)
        {
            platform.OnOut -= P_OnOut;

            if (isSuccess)
            {
                var message = await ClientBot?.SendTextMessageAsync(chat, "Сохраняем в Excel")!;

                await _platform?.SheetPostAsync()!;

                await client.EditMessageTextAsync(chat, message.MessageId, "Успешно сохранено!");
               
            } else await ClientBot?.SendTextMessageAsync(chat, "Операция отменена")!;

            _platform?.Dispose();
            _platform = null;
        }

        private static Task Update(ITelegramBotClient client, Update update, CancellationToken token)
        {
            if (update.Message is { } message && message.Text is { } text)
            {
                Console.WriteLine(text);
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
