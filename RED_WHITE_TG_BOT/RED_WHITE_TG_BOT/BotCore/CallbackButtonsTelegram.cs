using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types.ReplyMarkups;
using static System.Net.WebRequestMethods;

namespace RED_WHITE_TG_BOT.BotCore
{
    public static class CallbackButtonsTelegram
    {
        public static ReplyKeyboardMarkup Menu { get; } = new(new[]{
            new[]
            {
                new KeyboardButton("🔥 Старт"),
                new KeyboardButton("⭐️ Профиль")
            },
            new[]
            {
              new KeyboardButton("🤔 откуда бот"),
              new KeyboardButton("🤝 Реф. ссылка")
            },
            new[]
            {
                new KeyboardButton("💰 Получить бонусы")
            }
        })
        { 
            ResizeKeyboard = true
        };


        public static InlineKeyboardMarkup StartLogin { get; } = new(new[]{
            new[]
            {
                new InlineKeyboardButton("Канал")
                {
                    CallbackData = "channel",
                    Url ="https://t.me/bariga1313"
                }
            },
            new[]
            {
                new InlineKeyboardButton("Чат")
                {
                    CallbackData = "channel",
                    Url ="https://t.me/bariga13131313"
                }
            },
            new[]
            {
                new InlineKeyboardButton("Проверить")
                {
                    CallbackData = "checkLogin"
                }
            }
        });
    }
}
