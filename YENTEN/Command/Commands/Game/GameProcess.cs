using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Timers;
using Telegram.Bot;
using Telegram.Bot.Types;
using System.Linq;
using YENTEN.Command.CMDcommands;

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
            StartAndStopGame.GameStartTimer.Stop();
            StartAndStopGame.GameStartTimer.Dispose();
            //Очистка от мусор
            string queryString = "DELETE FROM CurrentGame WHERE  AmountYTN=0";
            DatabaseLibrary.ExecuteNonQuery(queryString);
            //Количество участников
            queryString = "SELECT COUNT(*) FROM CurrentGame WHERE Team = 0";
            int TeamHeadCount = DatabaseLibrary.ExecuteScalarInt(queryString);
            queryString = "SELECT COUNT(*) FROM CurrentGame WHERE Team = 1";
            int TeamTailsCount = DatabaseLibrary.ExecuteScalarInt(queryString);
            //
            //Запуск игры
            GameLogic(TeamHeadCount, TeamTailsCount);
            //
            //Создание новой игры
            queryString = "DELETE FROM CurrentGame";
            DatabaseLibrary.ExecuteNonQuery(queryString);
            queryString = "INSERT INTO CurrentGame VALUES('"+0+"','"+0+"','"+0+"')";
            DatabaseLibrary.ExecuteNonQuery(queryString);
            //
        }

        private static void GameLogic(int TeamHeadCount, int TeamTailsCount)
        {
            Console.WriteLine("Запуск игры!");
            connection = new SQLiteConnection("Data Source=MainDB1.db");
            SQLiteCommand Sqlcmd = connection.CreateCommand();
            //Парсер Random.org
            string urlAddress = "https://www.random.org/integers/?num=1&min=0&max=1&col=1&base=10&format=plain&rnd=new";
            int TeamWinID =Convert.ToInt32(BallanceCheck.getResponse(urlAddress));
            //
            //Подсчет суммы по командам     Количество игроков в командах     Head - орёл Tails- Решка
            decimal TeamHeadAmount = 0;
            decimal TeamTailsAmount = 0;
            string queryString = "SELECT max(rowid) FROM CurrentGame";
            int maxRowID = DatabaseLibrary.ExecuteScalarInt(queryString);
            queryString = "SELECT min(rowid) FROM CurrentGame";
            int minRowID = DatabaseLibrary.ExecuteScalarInt(queryString);
            for (int i = minRowID; i <= maxRowID; i++)
            {
                queryString = "SELECT AmountYTN FROM CurrentGame WHERE rowid=" + i;
                decimal Amount = DatabaseLibrary.ExecuteScalarDecimal(queryString);
                queryString = "SELECT Team FROM CurrentGame WHERE rowid=" + i;
                int TeamNumber = DatabaseLibrary.ExecuteScalarInt(queryString);
                if (TeamNumber == 0)
                {
                    TeamHeadAmount += Amount;
                }
                else
                {
                    TeamTailsAmount += Amount;
                }
            }
            int LoserCounter = 0;
            int WinnersCounter = 0;
            if (TeamWinID == 0)
            {
                string[] WinnersArray = new string[TeamHeadCount];
                decimal[] UserWinerAmount = new decimal[TeamHeadCount];
                decimal[] UserWinerBet = new decimal[TeamHeadCount];
                int[] UserWinerTelegramID = new int[TeamHeadCount];
                int counter = 0;
                //
                connection = new SQLiteConnection("Data Source=MainDB1.db");
                Sqlcmd = connection.CreateCommand();
                connection.Open();
                Sqlcmd.CommandText = "SELECT TelegramID, AmountYTN FROM CurrentGame WHERE Team=0";
                SQLiteDataReader reader = Sqlcmd.ExecuteReader();
                while (reader.Read())
                {
                    UserWinerTelegramID[counter] = Convert.ToInt32(reader["TelegramID"]);
                    UserWinerBet[counter] = Convert.ToDecimal(reader["AmountYTN"]);
                    counter++;
                }
                reader.Close();
                connection.Close();
                //
                for (int i = 0; i < TeamHeadCount; i++)
                {
                    //Считаем  выигрыш
                    decimal UserWinerPercent;
                    UserWinerPercent = (UserWinerBet[i] * 100) / TeamHeadAmount;
                    UserWinerAmount[i] = UserWinerBet[i] + TeamTailsAmount * (UserWinerPercent / 100);
                    //
                    UniversalLogic(connection, Sqlcmd, UserWinerTelegramID, UserWinerAmount, i);
                    //Записываем победителей
                    WinnersArray[WinnersCounter] = Convert.ToString(UserWinerTelegramID[i]) + "=(" + Convert.ToDecimal(UserWinerAmount[i]) + ":" + UserWinerBet[i] + ")";
                    WinnersCounter++;
                    //

                }
                //Запись данных проигравших в массивы
                string[] LosersArray = new string[TeamHeadCount];
                decimal[] UserLoserAmount = new decimal[TeamHeadCount];

                int[] UserLoserTelegramID = new int[TeamHeadCount];
                counter = 0;
                connection = new SQLiteConnection("Data Source=MainDB1.db");
                Sqlcmd = connection.CreateCommand();
                connection.Open();
                Sqlcmd.CommandText = "SELECT TelegramID, AmountYTN FROM CurrentGame WHERE Team=1";
                SQLiteDataReader reader2 = Sqlcmd.ExecuteReader();
                while (reader2.Read())
                {
                    UserLoserTelegramID[counter] = Convert.ToInt32(reader2["TelegramID"]);
                    UserLoserAmount[counter] = Convert.ToDecimal(reader2["AmountYTN"]);
                    counter++;
                }
                reader2.Close();
                connection.Close();
                //
                for (int i = 0; i < TeamTailsCount; i++)
                {
                    //Записываем проигравших
                    LosersArray[LoserCounter] = Convert.ToString(UserLoserTelegramID[i]) + "=(" + Convert.ToDecimal(UserLoserAmount[i]) + ")";
                    LoserCounter++;
                    //
                }
                //Переделываем массивы в строки
                string[] AllArray = new string[(LosersArray.Length + WinnersArray.Length)];
                LosersArray.CopyTo(AllArray, 0);
                WinnersArray.CopyTo(AllArray, LosersArray.Length);
                string Losers = string.Join(",", LosersArray);
                string Winners = string.Join(",", WinnersArray);
                string AllPlayers = string.Join(",", AllArray);
                RecordToHistory(connection, Sqlcmd, Losers, Winners, TeamWinID, AllPlayers);

            }
            else
            {
                string[] WinnersArray = new string[TeamTailsCount];
                decimal[] UserWinerAmount = new decimal[TeamTailsCount];
                decimal[] UserWinerBet = new decimal[TeamTailsCount];
                int[] UserWinerTelegramID = new int[TeamTailsCount];
                int counter = 0;

                //Запись данных победителей в массивы
                connection = new SQLiteConnection("Data Source=MainDB1.db");
                Sqlcmd = connection.CreateCommand();
                connection.Open();
                Sqlcmd.CommandText = "SELECT TelegramID, AmountYTN FROM CurrentGame WHERE Team=1";
                SQLiteDataReader reader4 = Sqlcmd.ExecuteReader();
                while (reader4.Read())
                {
                    UserWinerTelegramID[counter] = Convert.ToInt32(reader4["TelegramID"]);
                    UserWinerBet[counter] = Convert.ToDecimal(reader4["AmountYTN"]);
                    counter++;
                }
                reader4.Close();
                connection.Close();
                //
                for (int i = 0; i < TeamTailsCount; i++)
                {
                    //Считаем потенциальный выигрыш
                    decimal UserWinerPercent;
                    
                    UserWinerPercent = (UserWinerBet[i] * 100) / TeamTailsAmount;
                    UserWinerAmount[i] = UserWinerBet[i] + TeamHeadAmount * (UserWinerPercent / 100);
                    //

                    UniversalLogic(connection, Sqlcmd, UserWinerTelegramID, UserWinerAmount, i);
                    //Записываем победителей
                    WinnersArray[WinnersCounter] = Convert.ToString(UserWinerTelegramID[i])+"=("+Convert.ToDecimal(UserWinerAmount[i])+":"+ UserWinerBet[i]+")";
                    WinnersCounter++;
                    //
                }
                //Запись данных проигравших в массивы
                string[] LosersArray = new string[TeamHeadCount];
                decimal[] UserLoserAmount = new decimal[TeamHeadCount];
                int[] UserLoserTelegramID = new int[TeamHeadCount];
                counter = 0;
                connection = new SQLiteConnection("Data Source=MainDB1.db");
                connection.Open();
                Sqlcmd.CommandText = "SELECT TelegramID, AmountYTN FROM CurrentGame WHERE Team=0";
                SQLiteDataReader reader3 = Sqlcmd.ExecuteReader();
                while (reader3.Read())
                {
                    UserLoserTelegramID[counter] = Convert.ToInt32(reader3["TelegramID"]);
                    UserLoserAmount[counter] = Convert.ToDecimal(reader3["AmountYTN"]);
                    counter++;
                }
                reader3.Close();
                connection.Close();
                //
                for (int i = 0; i < TeamHeadCount; i++)
                {
                    //Записываем проигравших
                    LosersArray[LoserCounter] = Convert.ToString(UserLoserTelegramID[i]) + "=(" + Convert.ToDecimal(UserLoserAmount[i]) + ")";
                    LoserCounter++;
                    //
                }
                //Переделываем массивы в строки
                string[] AllArray = new string[(LosersArray.Length + WinnersArray.Length)];
                LosersArray.CopyTo(AllArray, 0);
                WinnersArray.CopyTo(AllArray, LosersArray.Length);
                string Losers = string.Join(",", LosersArray);
                string Winners = string.Join(",", WinnersArray);
                string AllPlayers = string.Join(",", AllArray);
                // Записываем в историю
                RecordToHistory(connection, Sqlcmd, Losers, Winners, TeamWinID, AllPlayers) ;
                //
            }
            //Добавляем в лог запись игры и выводим в консоль
            queryString = "SELECT max(GameID) FROM GameHistory";
            int GameID = DatabaseLibrary.ExecuteScalarInt(queryString);
            string appendText = "\n"+DateTime.Now + "  [Log]: Game № " +GameID+
                "\nКоличество участников: "+(TeamHeadCount+TeamTailsCount)
                +"\nКоличество участников команда Ореёл: "+TeamHeadCount+"  Баланс команды: "+TeamHeadAmount
                + "\nКоличество участников команда Решка: " + TeamTailsCount + "  Баланс команды: " + TeamTailsAmount
                + "\nОбщая ставка: "+(TeamHeadAmount+TeamTailsAmount)
                +"\nПобедила команда: "+TeamWinID + Environment.NewLine;

            System.IO.File.AppendAllText("log.txt", appendText);
            Console.WriteLine(appendText);
            //
            //Запуск таймера
            StartAndStopGame.StartGameCheck();
            //
        }

        private static void UniversalLogic(SQLiteConnection connection, SQLiteCommand Sqlcmd, int[] UserWinerTelegramID, decimal[] UserWinerAmount, int i)
        {
            //Добавляем выигрыш на счет
           string queryString = "SELECT WalletIN FROM UserInfo WHERE TelegramID=" + UserWinerTelegramID[i];
            string UserWinerWalletIN = DatabaseLibrary.ExecuteScalarString(queryString);
            queryString = "SELECT Ballance FROM BallanceCheck WHERE WalletIN=" + "'" + UserWinerWalletIN + "'";
            decimal UserWinerBallance = DatabaseLibrary.ExecuteScalarDecimal(queryString);
            connection = new SQLiteConnection("Data Source=MainDB1.db");
            Sqlcmd = connection.CreateCommand();
            connection.Open();
            Sqlcmd.CommandText = @"UPDATE BallanceCheck SET Ballance = :Ballance WHERE WalletIN=" + "'" + UserWinerWalletIN + "'";
            Sqlcmd.Parameters.Add("Ballance", System.Data.DbType.Decimal).Value = UserWinerBallance + UserWinerAmount[i];
            Sqlcmd.ExecuteNonQuery();
            connection.Close();
            //

        }


        private static void RecordToHistory(SQLiteConnection connection, SQLiteCommand Sqlcmd, string Losers, string Winners, int Team, string AllPlayers)
        {

            TimeZoneInfo moscowTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Russian Standard Time");
            DateTime moscowDateTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, moscowTimeZone);
            string queryString = "SELECT max(GameID) FROM GameHistory";
            int GameID = DatabaseLibrary.ExecuteScalarInt(queryString);
            queryString = "INSERT INTO GameHistory VALUES('"+ (GameID + 1)+"','"+Losers +"','"+ AllPlayers+"','"+Winners+"','"+ moscowTimeZone + "','"+Team+"','"+ 0+"')";
            DatabaseLibrary.ExecuteNonQuery(queryString);
        }


    }
}
