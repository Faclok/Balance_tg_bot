using Balance.GoogleSheet;
using Google.Apis.Sheets.v4.Data;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace Balance.Platforms
{
    public class Garantex(string googleSheetId, IGoogleSheetsManager googleSheets) : Platform
    {
        public override InlineKeyboardButton Main => MainGlobal;
        public static readonly InlineKeyboardButton MainGlobal = new(nameof(Garantex)) { CallbackData = nameof(Garantex) };

        private static readonly InlineKeyboardMarkup TryMoneyOut = new(
            new InlineKeyboardButton[]
            {
                new("Верно")
                {
                    CallbackData = SUCCESS_MONEY
                },
                new("Неверно")
                {
                    CallbackData = FAILED_MONEY
                }
            });

        private const string SUCCESS_MONEY = "success_money";
        private const string FAILED_MONEY = "failed_money";

        private const string SUM_ORDER = "sum_order";
        private const string SUM_PLUS = "sum_plus";
        private const string CURSE_SHARE = "curse_share";
        private const string CURSE_DEPOSIT = "curse_deposit";
        private const string CHECK_FORM = "check_form";

        private const string ERROR_PARSE_DATA = "Введите еще раз, бот не поддерживает такой формат";

        private string? currentState;

        private DateTime startClick;
        private float sumOrder = 0;
        private float sumPlus = 0f;
        private float curseShare = 0f;
        private const float sumCommission = 0.23f;
        private float curseDeposit = 0f;

        public override async Task SheetPostAsync()
        {
            var range = $"{nameof(Garantex)}!A:L";
            var valueRange = new ValueRange();

            var mountainRUB = Sentoke.SubtractPercentage(sumOrder, sumPlus);
            var USDT = mountainRUB / curseShare;
            var USDTCommission = USDT - sumCommission;
            var getRUB = curseDeposit * USDTCommission;

            var objectList = new List<object>() { $"{startClick:dd.MM.yy HH:mm}", MathF.Round(sumOrder, 3), MathF.Round(sumPlus, 3),  MathF.Round((float)mountainRUB, 3), Math.Round(curseShare, 3), MathF.Round((float)USDT, 3), MathF.Round(sumCommission, 3), MathF.Round((float)USDTCommission, 3), MathF.Round(curseDeposit, 3), MathF.Round((float)getRUB, 3), MathF.Round((float)(getRUB / sumOrder * 100f - 100f), 3), MathF.Round((float)getRUB - sumOrder, 3) };
            valueRange.Values = [objectList];

            await googleSheets.PostSpreadsheetAsync(googleSheetId, valueRange, range);
        }

        public override Task StartRG(ITelegramBotClient client, Chat chat, CancellationToken token)
        {
            startClick = DateTime.UtcNow + TimeSpan.FromHours(3);
            currentState = SUM_ORDER;
            return client.SendTextMessageAsync(chat, "Впишите сумму заявки (например: 10000)", cancellationToken: token);

        }

        public override Task UpdateTG(ITelegramBotClient client, Update update, CancellationToken token)
        {
            switch (update.Type)
            {
                case UpdateType.Message:
                    return UpdateMessage(client, update, token);

                case UpdateType.CallbackQuery:
                    return UpdateCallback(client, update, token);
            }

            return Task.CompletedTask;
        }

        private async Task UpdateMessage(ITelegramBotClient client, Update update, CancellationToken token)
        {
            if (update.Message is not { } message)
                return;

            if (message.Text is not { } text)
                return;

            switch (currentState)
            {
                case SUM_ORDER:
                    
                    if(float.TryParse(text, out sumOrder))
                    {
                        currentState = SUM_PLUS;
                        await client.SendTextMessageAsync(message.Chat, "Введите доплату % (например: 0,35 или 1)", cancellationToken: token);
                    } else await client.SendTextMessageAsync(message.Chat, ERROR_PARSE_DATA, cancellationToken: token);

                    break;

                case SUM_PLUS:

                    if (float.TryParse(text, out sumPlus))
                    {
                        currentState = CURSE_SHARE;
                        await client.SendTextMessageAsync(message.Chat, "Введите курс обмена на бирже(например: 90,07 или 91)", cancellationToken: token);
                    }
                    else await client.SendTextMessageAsync(message.Chat, ERROR_PARSE_DATA, cancellationToken: token);

                    break;

                case CURSE_SHARE:

                    if (float.TryParse(text, out curseShare))
                    {
                        await Console.Out.WriteLineAsync(curseShare.ToString());
                        currentState = CURSE_DEPOSIT;
                        await client.SendTextMessageAsync(message.Chat, "Введите курс пополнения на площадке (например: 90,84 или 93)", cancellationToken: token);
                    }
                    else await client.SendTextMessageAsync(message.Chat, ERROR_PARSE_DATA, cancellationToken: token);

                    break;

                case CURSE_DEPOSIT:

                    if (float.TryParse(text, out curseDeposit))
                    {
                        currentState = CHECK_FORM;
                        await client.SendTextMessageAsync(message.Chat, $"Сумма заявки: {sumOrder}\r\n Доплата {sumPlus}%\r\nКурс обмена на бирже: {curseShare}\r\nКомиссия на бирже: {sumCommission}\r\nКурс пополнения на площадке: {curseDeposit}",
                            replyMarkup: TryMoneyOut, cancellationToken: token);
                    }
                    else await client.SendTextMessageAsync(message.Chat, ERROR_PARSE_DATA, cancellationToken: token);

                    break;

                default:

                    break;
            }
        }

        private async Task UpdateCallback(ITelegramBotClient client, Update update, CancellationToken token)
        {
            if (update.CallbackQuery is not { } callbackQuery)
                return;

            if (callbackQuery.Data is not { } data)
                return;

            switch (data)
            {
                case SUCCESS_MONEY:
                    Out(client, callbackQuery.Message!.Chat,true);
                    break;

                case FAILED_MONEY:
                    Out(client, callbackQuery.Message!.Chat, false);
                    break;
            }

            await Task.CompletedTask;
        }
    }
}
