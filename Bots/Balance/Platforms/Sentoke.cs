using Balance.GoogleSheet;
using Google.Apis.Sheets.v4.Data;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace Balance.Platforms
{
    internal class Sentoke(string googleSheetId, IGoogleSheetsManager googleSheets) : Platform
    {
        public override InlineKeyboardButton Main => MainGlobal;
        public static readonly InlineKeyboardButton MainGlobal = new(nameof(Sentoke)) { CallbackData = nameof(Sentoke) };

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
        private const string CURSE_SHOP = "curse_shop";
        private const string PLUS_COMMISSION = "plus_commission";
        private const string CURSE_SELL = "curse_sell";
        private const string CHECK_FORM = "check_form";

        private const string ERROR_PARSE_DATA = "Введите еще раз, бот не поддерживает такой формат";

        private string? currentState;

        private DateTime startClick;
        private float sumOrder = 0f;
        private float curseShop = 0f;
        private float plusCommission = 0f;
        private float curseSell = 0f;

        public override async Task SheetPostAsync()
        {
            var range = $"{nameof(Sentoke)}!A:J";
            var valueRange = new ValueRange();

            var sumUSDT = sumOrder / curseShop;
            var sumNextCommission = SubtractPercentage(SubtractPercentage(sumUSDT, 1),1);
            var getOnRUB = curseSell * sumNextCommission - plusCommission;

            var objectList = new List<object>() { $"{startClick:dd.MM.yy HH:mm}", MathF.Round(sumOrder, 3), MathF.Round(curseShop, 3), Math.Round(sumUSDT, 3), MathF.Round((float)sumNextCommission, 3), MathF.Round(plusCommission, 3), MathF.Round(curseSell ,3), MathF.Round((float)getOnRUB, 3), MathF.Round((float)(getOnRUB / sumOrder * 100f - 100f), 3), MathF.Round((float)(getOnRUB - sumOrder), 3)};
            valueRange.Values = [objectList];

            await googleSheets.PostSpreadsheetAsync(googleSheetId, valueRange, range);
        }

        public static double SubtractPercentage(double number, double percent)
        {
            // Преобразование процента в дробное число (например, 1% становится 0.01)
            double percentDecimal = percent / 100.0;

            // Вычисление нового значения числа после вычитания процента
            double newNumber = number * (1 - percentDecimal);

            return newNumber;
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

                    if (float.TryParse(text, out sumOrder))
                    {
                        currentState = CURSE_SHOP;
                        await client.SendTextMessageAsync(message.Chat, "Введите курс покупки(например: 83,07)", cancellationToken: token);
                    }
                    else await client.SendTextMessageAsync(message.Chat, ERROR_PARSE_DATA, cancellationToken: token);

                    break;

                case CURSE_SHOP:

                    if (float.TryParse(text, out curseShop))
                    {
                        currentState = PLUS_COMMISSION;
                        await client.SendTextMessageAsync(message.Chat, "доп.комиссию(например: 100, 150)", cancellationToken: token);
                    }
                    else await client.SendTextMessageAsync(message.Chat, ERROR_PARSE_DATA, cancellationToken: token);

                    break;

                case PLUS_COMMISSION:

                    if (float.TryParse(text, out plusCommission))
                    {
                        currentState = CURSE_SELL;
                        await client.SendTextMessageAsync(message.Chat, "Введите курс продажи(указан на площадке, например: 90,76)", cancellationToken: token);
                    }
                    else await client.SendTextMessageAsync(message.Chat, ERROR_PARSE_DATA, cancellationToken: token);

                    break;

                case CURSE_SELL:

                    if (float.TryParse(text, out curseSell))
                    {
                        currentState = CHECK_FORM;
                        await client.SendTextMessageAsync(message.Chat, $"Сумма заявки:{sumOrder}\r\n Курс покупки: {curseShop}\r\nДоп комиссия: {plusCommission}\r\nКурс продажи: {curseSell}",
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
                    Out(client, callbackQuery.Message!.Chat, true);
                    break;

                case FAILED_MONEY:
                    Out(client, callbackQuery.Message!.Chat, false);
                    break;
            }

            await Task.CompletedTask;
        }
    }
}
