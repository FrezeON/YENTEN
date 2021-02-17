using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Text;
using System.Timers;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace YENTEN.Command.Commands.Game
{
    class GameProcess : Command
    {
        private static SQLiteConnection connection;
        public override string[] Names { get; set; }

        //Head(0) - орёл Tails(1)- Решка

        public override void Execute(Message message, TelegramBotClient client)
        {
        }
        public static void Run(Object source, ElapsedEventArgs e)
        {
            connection = new SQLiteConnection(@"Data Source=D:\YentLuckyBot\MainDB1.db");
            SQLiteCommand Sqlcmd = connection.CreateCommand();

            connection.Open();

            Sqlcmd.CommandText = "DELETE FROM CurrentGame WHERE  AmountYTN=0";
            Sqlcmd.ExecuteNonQuery();
            Sqlcmd.CommandText = "SELECT COUNT(*) FROM CurrentGame WHERE Team = 0";
            int TeamHeadCount = Convert.ToInt32(Sqlcmd.ExecuteScalar());
            Sqlcmd.CommandText = "SELECT COUNT(*) FROM CurrentGame WHERE Team = 1";
            int TeamTailsCount = Convert.ToInt32(Sqlcmd.ExecuteScalar());
            connection.Close();
            if (TeamHeadCount != 0 || TeamTailsCount != 0)
            {

            }
        }
    }
}
