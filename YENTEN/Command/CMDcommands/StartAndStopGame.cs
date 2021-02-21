using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Text;
using System.Timers;
using Telegram.Bot;
using Telegram.Bot.Types;
using YENTEN.Command.Commands.Game;

namespace YENTEN.Command.CMDcommands
{
    class StartAndStopGame : CMDcommand
    {
        public static Timer GameCheck;
        public static Timer GameStartTimer;
        private static SQLiteConnection connection;
        public override string[] Names { get; set; } = new string[] { "StartGame", "StopGame" };

        public override void Execute(string comandText)
        {
            if(comandText == "StartGame")
            {
                connection = new SQLiteConnection("Data Source=MainDB1.db");
                SQLiteCommand Sqlcmd = connection.CreateCommand();
                connection.Open();
                Sqlcmd.CommandText = "DELETE FROM NextGameTime";
                Sqlcmd.ExecuteNonQuery();
                Sqlcmd.CommandText = "INSERT INTO NextGameTime VALUES(@GameTime)";
                Sqlcmd.Parameters.AddWithValue("@GameTime", Convert.ToString(DateTime.Now.AddMinutes(5).ToShortTimeString()));
                Sqlcmd.ExecuteNonQuery();
                connection.Close();
                StartGameCheck();
            }
            else
            {
                GameCheck.Stop();
                GameCheck.Dispose();
            }
        }
        public static void StartGameCheck()
        {
            GameCheck = new System.Timers.Timer(5000);
            GameCheck.Elapsed += GameStart;
            GameCheck.AutoReset = true;
            GameCheck.Enabled = true;
        }
        
        private static void GameStart(Object source, ElapsedEventArgs e)
        {
            connection = new SQLiteConnection("Data Source=MainDB1.db");
            SQLiteCommand Sqlcmd = connection.CreateCommand();
            connection.Open();
            Sqlcmd.CommandText = "SELECT count(*) FROM CurrentGame WHERE  AmountYTN=0 AND  Team=0";
            int HeadAmountZERO = Convert.ToInt32(Sqlcmd.ExecuteScalar());
            Sqlcmd.CommandText = "SELECT count(*) FROM CurrentGame WHERE  AmountYTN=0 AND  Team=1";
            int TailsAmountZERO = Convert.ToInt32(Sqlcmd.ExecuteScalar());
            Sqlcmd.CommandText = "SELECT COUNT(*) FROM CurrentGame WHERE Team = 0";
            int TeamHeadCount = Convert.ToInt32(Sqlcmd.ExecuteScalar());
            Sqlcmd.CommandText = "SELECT COUNT(*) FROM CurrentGame WHERE Team = 1";
            int TeamTailsCount = Convert.ToInt32(Sqlcmd.ExecuteScalar());
            connection.Close();
            if ((TeamHeadCount- HeadAmountZERO) != 0 && (TeamTailsCount- TailsAmountZERO) != 0)
            {
                GameCheck.Stop();
                GameCheck.Dispose();
                connection.Open();
                Sqlcmd.CommandText = "DELETE FROM NextGameTime";
                Sqlcmd.ExecuteNonQuery();
                Sqlcmd.CommandText = "INSERT INTO NextGameTime VALUES(@GameTime)";
                Sqlcmd.Parameters.AddWithValue("@GameTime", Convert.ToString(DateTime.Now.AddMinutes(5).ToShortTimeString()));
                Sqlcmd.ExecuteNonQuery();
                connection.Close();
                GameStartTimer = new System.Timers.Timer(300000);
                GameStartTimer.Elapsed += GameProcess.Run;
                GameStartTimer.AutoReset = true;
                GameStartTimer.Enabled = true;
            }
        }
    }
}
