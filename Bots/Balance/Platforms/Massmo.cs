using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types;
using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;
using Balance.GoogleSheet;
using Google.Apis.Sheets.v4.Data;

namespace Balance.Platforms
{
    public class Massmo(string googleSheetId, IGoogleSheetsManager googleSheets) : Platform
    {

        #region Fields
        private const string PLATFORM_MONEY_SPB = "platform_money_spb";
        private const string PLATFORM_MONEY_MERGE_BANK = "platform_money_merge_bank";
        private const string PLATFORM_MONEY_NO_SALE = "platform_money_no_sale";
        private const string SUCCESS_MONEY = "success_money";
        private const string FAILED_MONEY = "failed_money";

        private DateTime startClick;
        private string Platform = string.Empty;
        private string? currentPlatform;
        private float Money = 0;
        private float camisa = 0f;
        private bool IsMoneyInput = false;

        #endregion

        #region Buttons

        private static readonly InlineKeyboardMarkup PlatformMoney = new(
        new InlineKeyboardButton[]
        {
                new("СПБ")
                {
                    CallbackData = PLATFORM_MONEY_SPB
                },
                new("Межбанк")
                {
                    CallbackData = PLATFORM_MONEY_MERGE_BANK
                },
                new("Без комиссии")
                {
                    CallbackData = PLATFORM_MONEY_NO_SALE
                }
        });

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

        public override InlineKeyboardButton Main => MainGlobal;
        public static readonly InlineKeyboardButton MainGlobal = new(nameof(Massmo)) { CallbackData =  nameof(Massmo) };

        #endregion

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

            if (IsMoneyInput)
            {
                if (float.TryParse(text, out var money))
                {
                    await client.SendTextMessageAsync(message.Chat, "Способ перевода:", replyMarkup: PlatformMoney, cancellationToken: token);
                    Money = money;
                    IsMoneyInput = false;
                }
                else
                    await client.SendTextMessageAsync(message.Chat, "К сожалению не удалось получить число. Попробуйте еще раз", cancellationToken: token);

                return;
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
           

                case PLATFORM_MONEY_SPB:
                    camisa = Convert.ToInt32(0.015f * Money);

                    currentPlatform = "СПБ";
                    Platform = PLATFORM_MONEY_SPB;
                    await client.SendTextMessageAsync(callbackQuery.Message!.Chat, $"Сумма перевода: {Money}\nСпособ оплаты: СПБ\nКомиссия: {camisa}\nИтого: {camisa + Money}", replyMarkup: TryMoneyOut, cancellationToken: token);
                    break;

                case PLATFORM_MONEY_MERGE_BANK:
                    camisa = Convert.ToInt32(0.02f * Money);

                    if (Money <= 2000)
                        camisa = 30;

                    currentPlatform = "Межбанк";
                    Platform = PLATFORM_MONEY_MERGE_BANK;
                    await client.SendTextMessageAsync(callbackQuery.Message!.Chat, $"Сумма перевода: {Money}\nСпособ оплаты: Межбанк\nКомиссия: {camisa}\nИтого: {camisa + Money}", replyMarkup: TryMoneyOut, cancellationToken: token);
                    break;

                case PLATFORM_MONEY_NO_SALE:

                    currentPlatform = "Без комиссии";
                    Platform = PLATFORM_MONEY_NO_SALE;
                    await client.SendTextMessageAsync(callbackQuery.Message!.Chat, $"Сумма перевода: {Money}\nСпособ оплаты: Без комиссии\nКомиссия: 0\nИтого: {Money}", replyMarkup: TryMoneyOut, cancellationToken: token);
                    break;

                case SUCCESS_MONEY:
                    Out(client, callbackQuery.Message!.Chat, true);
                    break;

                case FAILED_MONEY:
                    Out(client, callbackQuery.Message!.Chat, false);
                    break;
            }
        }

        public override async Task SheetPostAsync()
        {
            var range = $"{nameof(Massmo)}!A:E";
            var valueRange = new ValueRange();

            var sumCamisaMoney = camisa + Money;

            var objectList = new List<object>() { $"{startClick:dd.MM.yy HH:mm}", currentPlatform ?? "ERROR",  MathF.Round(Money, 3), MathF.Round(camisa, 3), MathF.Round((float)(Money + Sentoke.SubtractPercentage(Money, 98) - sumCamisaMoney), 3) };
            valueRange.Values = [objectList];

            await googleSheets.PostSpreadsheetAsync(googleSheetId, valueRange, range);
        }

        public override Task StartRG(ITelegramBotClient client, Chat chat, CancellationToken token)
        {
            startClick = DateTime.UtcNow + TimeSpan.FromHours(3);
            IsMoneyInput = true;
            return client.SendTextMessageAsync(chat, "Впишите сумму перевода:", cancellationToken: token);
        }
    }
}
