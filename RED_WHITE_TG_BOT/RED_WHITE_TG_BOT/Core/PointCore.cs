using RED_WHITE_TG_BOT.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RED_WHITE_TG_BOT.Core
{
    public static class PointCore
    {
        public static int MaxPoints { get; set; } = 150;

        public static void AddPoints(this TelegramUser user, int value)
        {
            user.Points += value;

            if(user.Points > MaxPoints)
                user.Points = MaxPoints;
        }

    }
}
