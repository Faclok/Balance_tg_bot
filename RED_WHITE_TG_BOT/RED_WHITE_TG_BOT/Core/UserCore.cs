using Microsoft.EntityFrameworkCore;
using RED_WHITE_TG_BOT.Model;
using RED_WHITE_TG_BOT.Services;
using Telegram.Bot;
using Telegram.Bot.Types;
using TimerEvent = System.Timers.Timer;

namespace RED_WHITE_TG_BOT.Core
{
    public static class UserCore
    {
        private readonly static List<long> _todayUpdate = [];
        private readonly static TimerEvent _timer;
        private readonly static UserService _userService;
        private readonly static SemaphoreSlim _semaphore = new(1);
        public const int MAX_POINTS = 150;

        static UserCore()
        {
            _userService = new();
            _timer = new((int)TimeSpan.FromDays(1).TotalMilliseconds);
            _timer.Elapsed += EveryDay;
            _timer.Start();
        }

        private static void EveryDay(object? sender, EventArgs e)
        {
            _todayUpdate.Clear();
        }

        public static async Task<int> UpdatePointsAsync(this TelegramUser user)
        {
            if (_todayUpdate.Any(o => o == user.ChatId))
                return 0;

            var randomValue = Random.Shared.Next(-7, 10);

            _todayUpdate.Add(user.ChatId);

            await _semaphore.WaitAsync();
            await _userService.Users.Where(o => o.ChatId == user.ChatId).ExecuteUpdateAsync(s => s.SetProperty(p => p.Points, p => p.Points + randomValue));
            _semaphore.Release();

            return randomValue;
        }

        public static bool CheckUpdateToday(this long chatId)
            => _todayUpdate.Any(o => o == chatId);

        public static bool CheckUpdateToday(this TelegramUser user)
            => _todayUpdate.Any(o => o == user.ChatId);

        public static async Task<bool> AnyAsync()
        {
            await _semaphore.WaitAsync();
            var result = await _userService.Users.AnyAsync();
            _semaphore.Release();

            return result;
        }

        public static async Task<TelegramUser?> TryGetUserAsync(this long chatId)
        {
            await _semaphore.WaitAsync();

            var result = await _userService.Users.AsNoTracking().FirstOrDefaultAsync(o => o.ChatId == chatId);

            _semaphore.Release();

            return result;
        }

        public static async Task<bool> TryCreateAccountAsync(this long chatId, string username)
        {

            await _semaphore.WaitAsync();

            if (await _userService.Users.AnyAsync(o => o.ChatId == chatId))
            {
                _semaphore.Release();
                return false;
            }

            var user = new TelegramUser()
            {
                ChatId = chatId,
                Username = username,
                Points = 0,
                CreateDate = DateTime.UtcNow,
            };

            await _userService.Users.AddAsync(user);
            await _userService.SaveChangesAsync();

            _semaphore.Release();

            return true;
        }

        public static async Task<TelegramUser[]> GetAllAsync()
        {
            await _semaphore.WaitAsync();
            var result = await _userService.Users.AsNoTracking().ToArrayAsync();
            _semaphore.Release();

            return result;
        }

        public static async Task<int> ClearPointAsync(int chatId, int? points)
        {
            await _semaphore.WaitAsync();

            var result = 0;
            if(points > 0)
                result = await _userService.Users.Where(o => o.ChatId == chatId).ExecuteUpdateAsync(s => s.SetProperty(p => p.Points, p => p.Points - points));
            else result = await _userService.Users.Where(o => o.ChatId == chatId).ExecuteUpdateAsync(s => s.SetProperty(p => p.Points, 0));

            _semaphore.Release();

            return result;
        }

        public static async Task AddPointsOnLinkAsync(this TelegramUser user)
        {
            await _semaphore.WaitAsync();

            await _userService.Users.Where(o => o.ChatId == user.ChatId).ExecuteUpdateAsync(s => s.SetProperty(p => p.Points, p => p.Points + 10));

            _semaphore.Release();
        }
    }
}
