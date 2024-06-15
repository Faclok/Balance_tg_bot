using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types;
using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;

namespace Balance.Platforms
{
    public class Massmo : Platform
    {

        #region Fields
        private const string PLATFORM_MONEY_SPB = "platform_money_spb";
        private const string PLATFORM_MONEY_MERGE_BANK = "platform_money_merge_bank";
        private const string PLATFORM_MONEY_NO_SALE = "platform_money_no_sale";
        private const string SUCCESS_MONEY = "success_money";
        private const string FAILED_MONEY = "failed_money";

        private bool IsMoneyInput = false;
        private int Money = 0;
        private string Platform = string.Empty;

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

        public override InlineKeyboardButton Main => _main;
        private static readonly InlineKeyboardButton _main = new(nameof(Massmo)) { CallbackData =  nameof(Massmo) };

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
                if (int.TryParse(text, out var money))
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
                    var camisaSPB = Convert.ToInt32(0.005f * Money);

                    Platform = PLATFORM_MONEY_SPB;
                    await client.SendTextMessageAsync(callbackQuery.Message!.Chat, $"Сумма перевода: {Money}\nСпособ оплаты: СПБ\nКомиссия: {camisaSPB}\nИтого: {camisaSPB + Money}", replyMarkup: TryMoneyOut, cancellationToken: token);
                    break;

                case PLATFORM_MONEY_MERGE_BANK:
                    var camisaBANK = Convert.ToInt32(0.015f * Money);

                    if (Money <= 2000)
                        camisaBANK = 30;

                    Platform = PLATFORM_MONEY_MERGE_BANK;
                    await client.SendTextMessageAsync(callbackQuery.Message!.Chat, $"Сумма перевода: {Money}\nСпособ оплаты: Межбанк\nКомиссия: {camisaBANK}\nИтого: {camisaBANK + Money}", replyMarkup: TryMoneyOut, cancellationToken: token);
                    break;

                case PLATFORM_MONEY_NO_SALE:
                    Platform = PLATFORM_MONEY_NO_SALE;
                    await client.SendTextMessageAsync(callbackQuery.Message!.Chat, $"Сумма перевода: {Money}\nСпособ оплаты: Без комиссии\nКомиссия: 0\nИтого: {Money}", replyMarkup: TryMoneyOut, cancellationToken: token);
                    break;

                case SUCCESS_MONEY:
                    Out(true);
                    break;

                case FAILED_MONEY:
                    Out(false);
                    break;
            }
        }

        public override Task SheetPostAsync()
        {
            throw new NotImplementedException();
        }

        public override Task StartRG(ITelegramBotClient client, Chat chat, CancellationToken token)
        {
            IsMoneyInput = true;
            return client.SendTextMessageAsync(chat, "Впишите сумму перевода:", cancellationToken: token);
        }
    }
}
