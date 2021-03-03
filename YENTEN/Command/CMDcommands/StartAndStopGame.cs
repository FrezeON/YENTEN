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
            TimeZoneInfo moscowTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Russian Standard Time");
            DateTime moscowDateTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, moscowTimeZone).AddMinutes(5);
            Console.WriteLine(moscowDateTime);
            if (comandText == "StartGame")
            {
                string queryString = "DELETE FROM NextGameTime";
                DatabaseLibrary.ExecuteNonQuery(queryString);
                queryString = "INSERT INTO NextGameTime VALUES('"+ Convert.ToString(moscowDateTime) +"');";
                DatabaseLibrary.ExecuteNonQuery(queryString);
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
            GameCheck.AutoReset = true;
            GameCheck.Enabled = true;
            GameCheck.Elapsed += GameStart;
        }
        
        private static void GameStart(Object source, ElapsedEventArgs e)
        {
            TimeZoneInfo moscowTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Russian Standard Time");
            DateTime moscowDateTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, moscowTimeZone).AddMinutes(5);
            string queryString = "SELECT count(*) FROM CurrentGame WHERE  AmountYTN=0 AND  Team=0";
            int HeadAmountZERO = DatabaseLibrary.ExecuteScalarInt(queryString);
            queryString = "SELECT count(*) FROM CurrentGame WHERE  AmountYTN=0 AND  Team=1";
            int TailsAmountZERO = DatabaseLibrary.ExecuteScalarInt(queryString);
            queryString = "SELECT COUNT(*) FROM CurrentGame WHERE Team = 0";
            int TeamHeadCount = DatabaseLibrary.ExecuteScalarInt(queryString);
            queryString = "SELECT COUNT(*) FROM CurrentGame WHERE Team = 1";
            int TeamTailsCount = DatabaseLibrary.ExecuteScalarInt(queryString);
            if ((TeamHeadCount- HeadAmountZERO) != 0 && (TeamTailsCount- TailsAmountZERO) != 0)
            {
                Console.WriteLine(moscowTimeZone +" [Log:] Конец раунда в:"+moscowDateTime);
                GameCheck.Stop();
                GameCheck.Dispose();
                queryString = "DELETE FROM NextGameTime";
                DatabaseLibrary.ExecuteNonQuery(queryString);
                queryString = "INSERT INTO NextGameTime VALUES('"+ Convert.ToString(moscowDateTime) +"');";
                DatabaseLibrary.ExecuteNonQuery(queryString);
                GameStartTimer = new System.Timers.Timer(300000);
                GameStartTimer.AutoReset = true;
                GameStartTimer.Enabled = true;
                GameStartTimer.Elapsed += GameProcess.Run;
            }
            else
            {
                GameCheck.Stop();
                StartGameCheck();
            }
        }
    }
}
