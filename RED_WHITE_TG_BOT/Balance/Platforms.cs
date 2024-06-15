using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace Balance
{
    public static class PlatformCollection
    {
        private static readonly List<Platform> _platforms = [];

        public static void PlatformAdd(Platform platform)
           => _platforms.Add(platform);

        public static void PlatformAdd(params Platform[] platform) 
           =>  _platforms.AddRange(platform);

        public static IEnumerable<Platform> GetPlatforms()
            => _platforms;

        public static event Action<Platform>? OnSelected;

        public static Task UpdateRG(ITelegramBotClient client, Update update, CancellationToken token)
        {
            if(update.CallbackQuery is { } callback && callback.Message is { } message)
            {
                var platform = _platforms.FirstOrDefault(o => o.Main.CallbackData == callback.Data);

                if (platform == null)
                    return Task.CompletedTask;

                OnSelected?.Invoke(platform);
                platform.StartRG(client, message.Chat, token);
            }

            return Task.CompletedTask;
        }
    }
}
