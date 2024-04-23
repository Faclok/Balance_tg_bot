using RED_WHITE_TG_BOT.Model;
using Telegram.Bot;
using TimerEvent = System.Timers.Timer;

namespace RED_WHITE_TG_BOT.Core
{
    public static class UserCore
    {

        private static List<TelegramUser> _users = [];
        private static List<TelegramUser> _todayUpdate = [];
        private static TimerEvent _timer;

        public static IEnumerable<TelegramUser> TelegramUsers => _users;

        static UserCore()
        {
            _timer = new((int)TimeSpan.FromDays(1).TotalMilliseconds);
            _timer.Elapsed += EveryDay;
            _timer.Start();
        }

        private static void EveryDay(object? sender, EventArgs e)
        {
            _todayUpdate.Clear();
        }

        public static int UpdatePoints(this TelegramUser user)
        {
            if (_todayUpdate.Any(o => o.ChatId == user.ChatId))
                return 0;

            var randomValue = Random.Shared.Next(-7, 10);

            _todayUpdate.Add(user);
            user.AddPoints(randomValue);

            return randomValue;
        }

        public static bool CheckUpdateToday(this long chatId)
            => _todayUpdate.Any(o => o.ChatId == chatId);

        public static bool CheckUpdateToday(this TelegramUser user)
            => _todayUpdate.Any(o => o.ChatId == user.ChatId);

        public static bool TryGetUser(this long chatId, out TelegramUser? user)
        {
            user = null;

            if (_users.FirstOrDefault(o => o.ChatId == chatId) is not { } value)
                return false;

            user = value;

            return true;
        }

        public static bool TryCreateAccount(this long chatId, string username)
        {
            if (_users.Any(o => o.ChatId == chatId))
                return false;

            var user = new TelegramUser()
            {
                ChatId = chatId,
                Username = username,
                Points = 0
            };

            _users.Add(user);

            return true;
        }
    }
}
