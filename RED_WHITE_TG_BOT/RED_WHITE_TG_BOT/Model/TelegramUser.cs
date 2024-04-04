using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RED_WHITE_TG_BOT.Model
{
    public class TelegramUser
    {
        public long ChatId { get; set; }
        public string? Username { get; set; }
        public int Points { get; set; }
    }
}
