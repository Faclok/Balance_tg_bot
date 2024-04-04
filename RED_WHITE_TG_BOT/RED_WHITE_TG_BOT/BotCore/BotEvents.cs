using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot;
using RED_WHITE_TG_BOT.Core;

namespace RED_WHITE_TG_BOT.BotCore
{
    public static class BotEvents
    {

        public static Task ListUsersCommandAsync(ITelegramBotClient client, Message message)
        {
            if (message.Chat is not { } chat)
                return Task.CompletedTask;

            if(!UserCore.TelegramUsers.Any())
                return Task.CompletedTask;

            return client.SendTextMessageAsync(chat.Id, string.Join('\n', UserCore.TelegramUsers.Select(o => $"ID: {o.ChatId}, NAME: {o.Username}, BALANCE: {o.Points}")));
        }

        public static Task ClearCommandAsync(ITelegramBotClient client, Message message)
        {
            if (message.Chat is not { } chat)
                return Task.CompletedTask;

            if (!UserCore.TelegramUsers.Any())
                return Task.CompletedTask;

            var id = message.Text?.Split(" ");

            if(UserCore.TelegramUsers.FirstOrDefault(o => o.ChatId.ToString() == id?.LastOrDefault()) is { } user)
            {
                user.Points = 0;
                return client.SendTextMessageAsync(chat.Id, "Успешная операция!");
            }

            return client.SendTextMessageAsync(chat.Id, "Такого пользовтеля нет!");
        }

        public static Task StartCommandAsync(ITelegramBotClient client, Message message)
        {
            if (message.Chat is not { } chat)
                return Task.CompletedTask;

            if (chat.Id.TryCreateAccount(chat.Username ?? "noname"))
                return client.SendTextMessageAsync(chat.Id, "Тут должно быть приветсвие вашего бота");

            if (!chat.Id.TryGetUser(out var user) || user == null)
                return client.SendTextMessageAsync(chat.Id, "you gey?");

            return client.SendTextMessageAsync(user.ChatId, $"Рад снова тебя видеть {user.Username}!");
        }

        public static Task ProfileCommandAsync(ITelegramBotClient client, Message message)
        {
            if (message.Chat is not { } chat)
                return Task.CompletedTask;

            if (!chat.Id.TryGetUser(out var user) || user == null)
                return client.SendTextMessageAsync(chat.Id, "you gey?");

            return client.SendTextMessageAsync(user.ChatId, $"Идентификатор: {user.ChatId}\nБаланс: {user.Points}", parseMode: Telegram.Bot.Types.Enums.ParseMode.Markdown);
        }


        public static Task PointsCommandAsync(ITelegramBotClient client, Message message)
        {
            if (message.Chat is not { } chat)
                return Task.CompletedTask;

            if (!chat.Id.TryGetUser(out var user) || user == null)
                return client.SendTextMessageAsync(chat.Id, "you gey?");

            if(user.CheckUpdateToday())
                return client.SendTextMessageAsync(user.ChatId, $"Сегодня вы уже получали свой бонус!");

            var value = user.UpdatePoints();
            return client.SendTextMessageAsync(user.ChatId, $"Вы успешно получили свой бонус!\nБонус: {value}\nВаш баланс: {user.Points}");
        }


        public static Task BotAboutCommandAsync(ITelegramBotClient client, Message message)
        {
            if (message.Chat is not { } chat)
                return Task.CompletedTask;

            return client.SendTextMessageAsync(chat.Id, $"Сделаем любого бота под вашу потребность!\n\nСПЕЦПРЕДЛОЖЕНИЕ ДО 1 МАЯ\nСкидка 70%\n\nПиши менеджеру: @qdcom\n\nПодробнее: @BotHubGroup");
        }

        public static Task DefaultCommandAsync(ITelegramBotClient client, Message message)
        {
            if (message.Chat is not { } chat)
                return Task.CompletedTask;

            return client.SendTextMessageAsync(chat.Id, $"Не совсем понял что вы хотите!");
        }
    }
}
