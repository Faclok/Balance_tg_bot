using RED_WHITE_TG_BOT.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using Telegram.Bot.Types.ReplyMarkups;

namespace RED_WHITE_TG_BOT.BotCore
{
    public static class CallbackButtonsTelegram
    {
        private readonly static ReplyKeyboardMarkup _menuSimple = new(new[]{
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

        private readonly static ReplyKeyboardMarkup _menuAdmin = new(new[]{
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
              new KeyboardButton("\U0001f921 Список участников"),
              new KeyboardButton("💩 Обнулить баллы")
            },
            new[]
            {
                new KeyboardButton("💰 Получить бонусы")
            }
        })
        {
            ResizeKeyboard = true
        };


        public static ReplyKeyboardMarkup GetMenu(TelegramUser? user)
        {
            if (user != null && user.ChatId == Program.ADMIN_ID)
                return _menuAdmin;

            return _menuSimple;
        }


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
