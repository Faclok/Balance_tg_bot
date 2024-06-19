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
    public static class PlatformCollection
    {
        private static readonly Dictionary<InlineKeyboardButton, Func<Platform>> _platforms = [];

        public static void PlatformAdd(InlineKeyboardButton key, Func<Platform> platform)
           => _platforms.Add(key, platform);

        public static IEnumerable<InlineKeyboardButton> GetMains()
            => _platforms.Select(o => o.Key);

        public static event Action<Platform>? OnSelected;

        public static Task UpdateRG(ITelegramBotClient client, Update update, CancellationToken token)
        {
            if(update.CallbackQuery is { } callback && callback.Message is { } message)
            {
                var platform = _platforms.FirstOrDefault(o => o.Key.CallbackData == callback.Data);

                if (platform.Key?.CallbackData is not { } data || data != callback.Data)
                    return Task.CompletedTask;

                var platformValue = platform.Value();
                OnSelected?.Invoke(platformValue);
                platformValue.StartRG(client, message.Chat, token);
            }

            return Task.CompletedTask;
        }
    }
}
