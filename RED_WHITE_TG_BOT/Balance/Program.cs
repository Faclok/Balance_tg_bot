using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types;
using Balance.Platforms;
using Telegram.Bot.Types.ReplyMarkups;

namespace Balance
{
    internal class Program
    {
        public static ITelegramBotClient? ClientBot { get; private set; }
        public static string? BotName { get; private set; }

        public const string UsernameRoot = "pofig_na_teba";

        private static Platform? _platform;

        static async Task Main(string[] args)
        {
            PlatformCollection.OnSelected += (p) => {
                p.OnOut += P_OnOut;
                _platform = p;
            };
            
            PlatformCollection.PlatformAdd(new Massmo(), new Garantex(), new Sentoke());

            await Console.Out.WriteLineAsync("start");
            ClientBot = new TelegramBotClient("7007363424:AAFNHUTw5FFQ9EyDzIkVH2uYVAYkdQDFQ8g");
            BotName = (await ClientBot.GetMeAsync()).Username;
            await Console.Out.WriteLineAsync("my name: " + BotName);

            ClientBot.StartReceiving(Update, Exception);

            await Task.Delay(Timeout.Infinite);
        }

        private static void P_OnOut(bool isSuccess)
        {
            throw new NotImplementedException();
        }

        private static Task Update(ITelegramBotClient client, Update update, CancellationToken token)
        {
            if (update.Message is { } message && message.Text is { } text)
            {
                if (text == "/start" && _platform == null)
                    return client.SendTextMessageAsync(message.Chat, "Добрый день!", replyMarkup: new InlineKeyboardMarkup(PlatformCollection.GetPlatforms().Select(o => o.Main)),
                        cancellationToken: token);
                else if(text == "/clear" && _platform != null)
                {
                    _platform.Dispose();
                    _platform = null;

                    return client.SendTextMessageAsync(message.Chat, "Операция отменена!", replyMarkup: new InlineKeyboardMarkup(PlatformCollection.GetPlatforms().Select(o => o.Main)),
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
