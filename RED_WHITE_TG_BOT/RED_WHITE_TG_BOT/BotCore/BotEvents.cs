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

        public static Task ListUsersCommandAsync(ITelegramBotClient client, Message message)
        {
            if (message.Chat is not { } chat)
                return Task.CompletedTask;

            if (!UserCore.TelegramUsers.Any())
                return Task.CompletedTask;

            return client.SendTextMessageAsync(chat.Id, string.Join('\n', UserCore.TelegramUsers.Select(o => $"ID: `{o.ChatId}`, NAME: {o.Username}, BALANCE: {o.Points}")), parseMode: Telegram.Bot.Types.Enums.ParseMode.MarkdownV2);
        }

        public static Task ClearCommandAsync(ITelegramBotClient client, Message message)
        {
            if (message.Chat is not { } chat)
                return Task.CompletedTask;

            if (!UserCore.TelegramUsers.Any())
                return Task.CompletedTask;

            var id = message.Text?.Split(" ");

            if (UserCore.TelegramUsers.FirstOrDefault(o => o.ChatId.ToString() == id?.LastOrDefault()) is { } user)
            {
                user.Points = 0;
                return client.SendTextMessageAsync(chat.Id, "Успешная операция!");
            }

            return client.SendTextMessageAsync(chat.Id, "Такого пользовтеля нет!");
        }

        public static Task MenuCommandAsync(ITelegramBotClient client, Message message)
            => client.SendTextMessageAsync(message.Chat, "Вы находитесь в меню", replyMarkup: CallbackButtonsTelegram.GetMenu((message?.Chat?.Id.TryGetUser(out var user) ?? false && user != null) ? user : null));

        public static Task StartCommandAsync(ITelegramBotClient client, Message message)
        {
            if (message.Chat is not { } chat)
                return Task.CompletedTask;

            InlineKeyboardMarkup? buttons = null;

            if (chat.Id.TryCreateAccount(chat.Username ?? "noname"))
            {
                if (message.Text is { } text)
                {
                    var split = text.Split(' ');
                    if (split.Length > 1 && long.TryParse(split[1], out var parseId) && parseId.TryGetUser(out var userLink) && userLink != null)
                        userLink.AddPoints(10);
                }

                buttons = CallbackButtonsTelegram.StartLogin;
            }

            var path = Path.GetFullPath(_imageChannelPath);
            return client.SendPhotoAsync(chat.Id, InputFile.FromStream(System.IO.File.OpenRead(path)), caption: "👋 Вас приветствует магазин Red&White Ukhta\r\n💸 С помощью меня ты можешь копить бонусы на покупки в нашем магазине.\r\nВ дальнейшем вы сможете сделать заказ через этого бота\r\nЧтобы начать копить бонусы, нужно подписаться на наш канал и вступить в чат", replyMarkup: buttons);
        }

        public static Task ProfileCommandAsync(ITelegramBotClient client, Message message)
        {
            if (message.Chat is not { } chat)
                return Task.CompletedTask;

            if (!chat.Id.TryGetUser(out var user) || user == null)
                return client.SendTextMessageAsync(chat.Id, "you gey?");

            return client.SendTextMessageAsync(user.ChatId, $"🤖 ID: {user.ChatId}\n💰 Баланс: {user.Points}", parseMode: Telegram.Bot.Types.Enums.ParseMode.Markdown);
        }

        public static Task PointsCommandAsync(ITelegramBotClient client, Message message)
        {
            if (message.Chat is not { } chat)
                return Task.CompletedTask;

            if (!chat.Id.TryGetUser(out var user) || user == null)
                return client.SendTextMessageAsync(chat.Id, "you gey?");

            if (user.CheckUpdateToday())
                return client.SendTextMessageAsync(user.ChatId, $"Сегодня вы уже получали свой бонус!");

            var value = user.UpdatePoints();

            return client.SendTextMessageAsync(user.ChatId,
                value > 0 ? $"Вы успешно получили свой бонус, ждём вас завтра!\n🔥 Получено: +{value}\n💰 Ваш баланс: {user.Points}"
                : $"Сегодня к сожалению тебе не повезло, ждём вас завтра!\n🔥 Получено: {value}\n💰 Ваш баланс: {user.Points}");

        }

        public static Task BotAboutCommandAsync(ITelegramBotClient client, Message message)
        {
            if (message.Chat is not { } chat)
                return Task.CompletedTask;

            var path = Path.GetFullPath(_imageBotHubPath);
            return client.SendPhotoAsync(chat.Id, InputFile.FromStream(System.IO.File.OpenRead(path)), caption: $"Сделаем любого бота под вашу потребность!\n\nСПЕЦПРЕДЛОЖЕНИЕ ДО 1 МАЯ\nСкидка 70%\n\nПиши менеджеру: @qdcom\n\nПодробнее: @BotHubGroup");
        }

        public static Task SendLinkCommandAsync(ITelegramBotClient client, Message message)
        {
            if (message.Chat is not { } chat)
                return Task.CompletedTask;

            if (!chat.Id.TryGetUser(out var user) || user == null)
                return Task.CompletedTask;

            return client.SendTextMessageAsync(user.ChatId, $"Всем салам алейкум от основателя R&W Герыча, Гарей, как вам удобно.\nНадеюсь вы уже разобрались что и как в нашем боте и для чего он вообще был сделан.\nКонечно мы не забыли про индивидуальную реферальную ссылку: [https://t.me/{Program.BotName}?start={user.ChatId}](https://t.me/{Program.BotName}?start={user.ChatId})\nЗа каждого приглашенного вами друга, вам будет капать в бота 10 бонусов, которые равны 10 рублям.\nНадеюсь всем все понятно, но если вопросы все же возникнут, то обращайтесь к главному админу, всем отвечу.\nВсех люблю❤️\nP.s. Админ [R&W](https://t.me/bariga1313)", parseMode: Telegram.Bot.Types.Enums.ParseMode.Markdown);
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
