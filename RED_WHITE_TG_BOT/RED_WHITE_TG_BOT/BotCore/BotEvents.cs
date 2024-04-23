using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot;
using RED_WHITE_TG_BOT.Core;
using Telegram.Bot.Types.ReplyMarkups;

namespace RED_WHITE_TG_BOT.BotCore
{
    public static class BotEvents
    {
        private const string _imageBotHubPath = @"Images\photo_2024-04-06_23-25-09.jpg";
        private const string _imageChannelPath = @"Images\photo_2024-04-06_23-44-28.jpg";

        public static async Task ListUsersCommandAsync(ITelegramBotClient client, Message message)
        {
            if (message.Chat is not { } chat)
                return;

            if (!await UserCore.AnyAsync())
                return;

            await client.SendTextMessageAsync(chat.Id, string.Join('\n', (await UserCore.GetAllAsync()).Select(o => $"ID: `{o.ChatId}`, NAME: {o.Username}, BALANCE: {o.Points}")), parseMode: Telegram.Bot.Types.Enums.ParseMode.MarkdownV2);
        }

        public static async Task ClearCommandAsync(ITelegramBotClient client, Message message)
        {
            if (message.Chat is not { } chat)
                return;

            if (!await UserCore.AnyAsync())
                return;

            var data = message.Text?.Split(" ");

            if (data == null || data.Length <= 1)
            {
                await client.SendTextMessageAsync(chat.Id, "Такого пользователя нет!");
                return;
            }

            var removePoints = -1;

            if (data.Length >= 3)
                removePoints = int.TryParse(data[2], out var parseR) ? parseR : -1;

            if (int.TryParse(data[1], out var parse) && await UserCore.ClearPointAsync(parse, removePoints) > 0)
                await client.SendTextMessageAsync(chat.Id, "Успешная операция!");
            else await client.SendTextMessageAsync(chat.Id, "Такого пользователя нет!");
        }

        public static async Task MenuCommandAsync(ITelegramBotClient client, Message message)
            => await client.SendTextMessageAsync(message.Chat, "Вы находитесь в меню", replyMarkup: CallbackButtonsTelegram.GetMenu(await message.Chat.Id.TryGetUserAsync()));

        public static async Task StartCommandAsync(ITelegramBotClient client, Message message)
        {
            if (message.Chat is not { } chat)
                return;

            InlineKeyboardMarkup? buttons = null;

            if (await chat.Id.TryCreateAccountAsync(chat.Username ?? "noname"))
            {
                if (message.Text is { } text)
                {
                    var split = text.Split(' ');
                    if (split.Length > 1 && long.TryParse(split[1], out var parseId) && await parseId.TryGetUserAsync() is { } userLink)
                        await userLink.AddPointsOnLinkAsync();
                }

                buttons = CallbackButtonsTelegram.StartLogin;
            }

            var path = Path.GetFullPath(_imageChannelPath);
            await client.SendPhotoAsync(chat.Id, InputFile.FromStream(System.IO.File.OpenRead(path)), caption: "👋 Вас приветствует магазин Red&White Ukhta\r\n💸 С помощью меня ты можешь копить бонусы на покупки в нашем магазине.\r\nВ дальнейшем вы сможете сделать заказ через этого бота\r\nЧтобы начать копить бонусы, нужно подписаться на наш канал и вступить в чат", replyMarkup: buttons);
        }

        public static async Task ProfileCommandAsync(ITelegramBotClient client, Message message)
        {
            if (message.Chat is not { } chat)
                return;

            if (await chat.Id.TryGetUserAsync() is not { } user)
            {
                await client.SendTextMessageAsync(chat.Id, "you gey?");
                return;
            }

            await client.SendTextMessageAsync(user.ChatId, $"🤖 ID: {user.ChatId}\n💰 Баланс: {user.Points}", parseMode: Telegram.Bot.Types.Enums.ParseMode.Markdown);
        }

        public static async Task PointsCommandAsync(ITelegramBotClient client, Message message)
        {
            if (message.Chat is not { } chat)
                return;

            if (await chat.Id.TryGetUserAsync() is not { } user)
            {
                await client.SendTextMessageAsync(chat.Id, "you gey?");
                return;
            }

            if (user.CheckUpdateToday())
               {
                 await client.SendTextMessageAsync(user.ChatId, $"Сегодня вы уже получали свой бонус!");
                return;
            }

            var value = await user.UpdatePointsAsync();

            await client.SendTextMessageAsync(user.ChatId,
                value > 0 ? $"Вы успешно получили свой бонус, ждём вас завтра!\n🔥 Получено: +{value}\n💰 Ваш баланс: {user.Points + value}"
                : $"Сегодня к сожалению тебе не повезло, ждём вас завтра!\n🔥 Получено: {value}\n💰 Ваш баланс: {user.Points}");

        }

        public static Task BotAboutCommandAsync(ITelegramBotClient client, Message message)
        {
            if (message.Chat is not { } chat)
                return Task.CompletedTask;

            var path = Path.GetFullPath(_imageBotHubPath);
            return client.SendPhotoAsync(chat.Id, InputFile.FromStream(System.IO.File.OpenRead(path)), caption: $"Сделаем любого бота под вашу потребность!\n\nСПЕЦПРЕДЛОЖЕНИЕ ДО 1 МАЯ\nСкидка 70%\n\nПиши менеджеру: @qdcom\n\nПодробнее: @BotHubGroup");
        }

        public static async Task SendLinkCommandAsync(ITelegramBotClient client, Message message)
        {
            if (message.Chat is not { } chat)
                return;

            if (await chat.Id.TryGetUserAsync() is not { } user)
                return;

            await client.SendTextMessageAsync(user.ChatId, $"Всем салам алейкум от основателя R&W Герыча, Гарей, как вам удобно.\nНадеюсь вы уже разобрались что и как в нашем боте и для чего он вообще был сделан.\nКонечно мы не забыли про индивидуальную реферальную ссылку: [https://t.me/{Program.BotName}?start={user.ChatId}](https://t.me/{Program.BotName}?start={user.ChatId})\nЗа каждого приглашенного вами друга, вам будет капать в бота 10 бонусов, которые равны 10 рублям.\nНадеюсь всем все понятно, но если вопросы все же возникнут, то обращайтесь к главному админу, всем отвечу.\nВсех люблю❤️\nP.s. Админ [R&W](https://t.me/bariga1313)", parseMode: Telegram.Bot.Types.Enums.ParseMode.Markdown);
        }

        public static Task NoLoginAsync(ITelegramBotClient client, Message message)
        {
            if (message.Chat is not { } chat)
                return Task.CompletedTask;

            return client.SendTextMessageAsync(chat.Id, $"Что бы продолжить, нужно подписаться!", replyMarkup: CallbackButtonsTelegram.StartLogin);
        }

        public static Task DefaultCommandAsync(ITelegramBotClient client, Message message)
        {
            if (message.Chat is not { } chat)
                return Task.CompletedTask;

            return client.SendTextMessageAsync(chat.Id, $"Не совсем понял что вы хотите!");
        }
    }
}
