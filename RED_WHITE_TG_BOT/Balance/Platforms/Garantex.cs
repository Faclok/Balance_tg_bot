using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace Balance.Platforms
{
    public class Garantex : Platform
    {
        public override InlineKeyboardButton Main => _main;
        private static readonly InlineKeyboardButton _main = new(nameof(Garantex)) { CallbackData = nameof(Garantex) };

        public override Task SheetPostAsync()
        {
            throw new NotImplementedException();
        }

        public override Task StartRG(ITelegramBotClient client, Chat chat, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        public override Task UpdateTG(ITelegramBotClient client, Update update, CancellationToken token)
        {
            throw new NotImplementedException();
        }
    }
}
