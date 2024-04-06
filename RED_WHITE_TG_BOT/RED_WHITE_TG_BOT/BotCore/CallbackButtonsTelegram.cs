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
        public static InlineKeyboardMarkup Menu { get; } = new(new[]{
            new[]
            {
                new InlineKeyboardButton("профиль")
                {
                    CallbackData = "profile"
                }
            },
            new[]
            {
               new InlineKeyboardButton("Собрать бонусы")
               {
                    CallbackData = "points"
               },

            },
            new[]
            {
              new InlineKeyboardButton("Помощь")
              {
                    CallbackData = "help"
              },
            },
            new[]
            {
                new InlineKeyboardButton("От куда бот?")
                {
                    CallbackData = "bot"
                }
            }
        });

        public static InlineKeyboardMarkup BackToMenu { get; } = new(new[]{
            new[]
            {
                new InlineKeyboardButton("меню")
                {
                    CallbackData = "menu"
                }
            }
        });

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
