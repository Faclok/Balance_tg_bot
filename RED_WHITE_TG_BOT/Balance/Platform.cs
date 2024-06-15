using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace Balance
{
    public abstract class Platform : IDisposable
    {
        public abstract InlineKeyboardButton Main { get; }

        public event Action<bool>? OnOut;

        private protected void Out(bool isSuccess)
          => OnOut?.Invoke(isSuccess);

        public abstract Task StartRG(ITelegramBotClient client, Chat chat, CancellationToken token);

        public abstract Task UpdateTG(ITelegramBotClient client, Update update, CancellationToken token);

        public abstract Task SheetPostAsync();

        public void Dispose() => OnOut = null;
    }

}
