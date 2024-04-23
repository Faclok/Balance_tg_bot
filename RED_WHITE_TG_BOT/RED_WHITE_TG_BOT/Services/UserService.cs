using Microsoft.EntityFrameworkCore;
using RED_WHITE_TG_BOT.Model;

namespace RED_WHITE_TG_BOT.Services
{
    public class UserService : DbContext
    {
        #region TABLES

        public DbSet<TelegramUser> Users { get; set; }
        #endregion

        //#region CONSTRUCTOR

        //parameterless constructor must be above the others,
        //as it seems that EF Tools migrator just takes the .First() of them

        public const string ConnectionString = "server=rc1b-n4kvp3yklxkjhnvz.mdb.yandexcloud.net;port=3306;database=telegram;user=Kvantorianec;password=uzer1pass;default command timeout=60;CharSet=utf8;";

        /// <summary>
        /// Constructor for creating migrations
        /// </summary>
        public UserService()
        {
            File = ConnectionString;

            Initialize();
        }

        void Initialize()
        {
            if (!Initialized)
            {
                Initialized = true;

                Database.EnsureCreated();
            }
        }

        public static string File { get; protected set; } = string.Empty;
        public static bool Initialized { get; protected set; }


        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder
                .UseMySql(File, ServerVersion.AutoDetect(File));
        }

        public void Reload()
        {
            Database.CloseConnection();
            Database.OpenConnection();
        }

    }
}
