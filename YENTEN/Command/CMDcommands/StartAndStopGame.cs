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
        private static Timer aTimer;
        private static SQLiteConnection connection;
        public override string[] Names { get; set; } = new string[] { "StartGame", "StopGame" };

        public override void Execute(string comandText)
        {
            if(comandText == "StartGame")
            {
                connection = new SQLiteConnection("Data Source=MainDB1.db");
                SQLiteCommand Sqlcmd = connection.CreateCommand();
                string TimeStart =  DateTime.Now.ToString("HH") + ":" + Convert.ToString(DateTime.Now.Minute + 5);
                connection.Open();
                Sqlcmd.CommandText = "DELETE FROM NextGameTime";
                Sqlcmd.ExecuteNonQuery();
                Sqlcmd.CommandText = "INSERT INTO NextGameTime VALUES(@GameTime)";
                Sqlcmd.Parameters.AddWithValue("@GameTime",TimeStart);
                Sqlcmd.ExecuteNonQuery();
                connection.Close();
                SetTimer();
            }
            else
            {
                aTimer.Stop();
                aTimer.Dispose();
            }
        }
        public static void SetTimer()
        {
            aTimer = new System.Timers.Timer(300000);
            aTimer.Elapsed += GameProcess.Run;
            aTimer.AutoReset = true;
            aTimer.Enabled = true;
        }
    }
}
