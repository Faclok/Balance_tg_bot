using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace RED_WHITE_TG_BOT.BotCore
{
    public class CommandTelegram(BotCommand command, Func<ITelegramBotClient, Message, Task> callback)
    {
        public event Action<ITelegramBotClient, Message>? OnMessage;
        private readonly Func<ITelegramBotClient, Message, Task> _callback = callback;
        public readonly BotCommand BotCommand = command;

        public async Task InvokeAsync(ITelegramBotClient bot, Message message)
        {
            await _callback(bot, message);
            OnMessage?.Invoke(bot, message);
        }
    }
}
