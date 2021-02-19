using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Timers;
using Telegram.Bot;
using Telegram.Bot.Types;
using System.Linq;

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


            connection = new SQLiteConnection("Data Source=MainDB1.db");
            SQLiteCommand Sqlcmd = connection.CreateCommand();

            connection.Open();
            Sqlcmd.CommandText = "DELETE FROM CurrentGame WHERE  AmountYTN=0";
            Sqlcmd.ExecuteNonQuery();
            Sqlcmd.CommandText = "SELECT COUNT(*) FROM CurrentGame WHERE Team = 0";
            int TeamHeadCount = Convert.ToInt32(Sqlcmd.ExecuteScalar());
            Sqlcmd.CommandText = "SELECT COUNT(*) FROM CurrentGame WHERE Team = 1";
            int TeamTailsCount = Convert.ToInt32(Sqlcmd.ExecuteScalar());
            connection.Close();
            if (TeamHeadCount != 0 && TeamTailsCount != 0)
            {
                GameLogic(connection, Sqlcmd, TeamHeadCount, TeamTailsCount);
                connection.Open();
                Sqlcmd.CommandText ="DELETE FROM CurrentGame";
                Sqlcmd.ExecuteNonQuery();
                Sqlcmd.CommandText = "INSERT INTO CurrentGame VALUES(@TelegramID, @AmountYTN,@Team)";
                Sqlcmd.Parameters.AddWithValue("@TelegramID", 0);
                Sqlcmd.Parameters.AddWithValue("@AmountYTN", 0);
                Sqlcmd.Parameters.AddWithValue("@Team", 0);
                Sqlcmd.ExecuteNonQuery();
                connection.Close();
            }
            else
            {
                connection.Open();
                Sqlcmd.CommandText = "INSERT INTO CurrentGame VALUES(@TelegramID, @AmountYTN,@Team)";
                Sqlcmd.Parameters.AddWithValue("@TelegramID", 0);
                Sqlcmd.Parameters.AddWithValue("@AmountYTN", 0);
                Sqlcmd.Parameters.AddWithValue("@Team", 0);
                Sqlcmd.ExecuteNonQuery();
                connection.Close();
            }
            
        }

        private static void GameLogic(SQLiteConnection connection, SQLiteCommand Sqlcmd, int TeamHeadCount, int TeamTailsCount)
        {
            //Парсер Random.org
            string urlAddress = "https://www.random.org/integers/?num=1&min=0&max=1&col=1&base=10&format=plain&rnd=new";
            int TeamWinID =Convert.ToInt32(BallanceCheck.getResponse(urlAddress));
            //

            //Подсчет суммы по командам     Количество игроков в командах     Head - орёл Tails- Решка
            double TeamHeadAmount = 0;
            double TeamTailsAmount = 0;
            connection.Open();
            Sqlcmd.CommandText = "SELECT max(rowid) FROM CurrentGame";
            int maxRowID = Convert.ToInt32(Sqlcmd.ExecuteScalar());
            Sqlcmd.CommandText = "SELECT min(rowid) FROM CurrentGame";
            int minRowID = Convert.ToInt32(Sqlcmd.ExecuteScalar());
            for (int i = minRowID; i <= maxRowID; i++)
            {
                Sqlcmd.CommandText = "SELECT AmountYTN FROM CurrentGame WHERE rowid=" + i;
                double Amount = Convert.ToDouble(Sqlcmd.ExecuteScalar());
                Sqlcmd.CommandText = "SELECT Team FROM CurrentGame WHERE rowid=" + i;
                int TeamNumber = Convert.ToInt32(Sqlcmd.ExecuteScalar());
                if (TeamNumber == 0)
                {
                    TeamHeadAmount += Amount;
                }
                else
                {
                    TeamTailsAmount += Amount;
                }
            }
            connection.Close();
            int LoserCounter = 0;
            int WinnersCounter = 0;
            if (TeamWinID == 0)
            {
                string[] WinnersArray = new string[TeamHeadCount];
                double[] UserWinerAmount = new double[TeamHeadCount];
                int[] UserWinerTelegramID = new int[TeamHeadCount];
                int counter = 0;
                //
                connection.Open();
                Sqlcmd.CommandText = "SELECT TelegramID, AmountYTN FROM CurrentGame WHERE Team=0";
                SQLiteDataReader reader = Sqlcmd.ExecuteReader();
                while (reader.Read())
                {
                    UserWinerTelegramID[counter] = Convert.ToInt32(reader["TelegramID"]);
                    UserWinerAmount[counter] = Convert.ToDouble(reader["AmountYTN"]);
                    counter++;
                }
                reader.Close();
                connection.Close();
                //
                for (int i = 0; i < TeamHeadCount; i++)
                {
                    //Считаем  выигрыш
                    double UserWinerPercent;
                    UserWinerPercent = (UserWinerAmount[i] * 100) / TeamHeadAmount;
                    UserWinerAmount[i] = UserWinerAmount[i] + TeamTailsAmount * (UserWinerPercent / 100);
                    //
                    UniversalLogic(connection, Sqlcmd, UserWinerTelegramID, UserWinerAmount, i);
                    //Записываем победителей
                    WinnersArray[WinnersCounter] = Convert.ToString(UserWinerTelegramID[i]) + "=(" + Convert.ToDouble(UserWinerAmount[i]) + ")"; ;
                    WinnersCounter++;
                    //

                }
                //Запись данных проигравших в массивы
                string[] LosersArray = new string[TeamHeadCount];
                double[] UserLoserAmount = new double[TeamHeadCount];
                int[] UserLoserTelegramID = new int[TeamHeadCount];
                counter = 0;
                connection.Open();
                Sqlcmd.CommandText = "SELECT TelegramID, AmountYTN FROM CurrentGame WHERE Team=1";
                SQLiteDataReader reader2 = Sqlcmd.ExecuteReader();
                while (reader2.Read())
                {
                    UserLoserTelegramID[counter] = Convert.ToInt32(reader2["TelegramID"]);
                    UserLoserAmount[counter] = Convert.ToDouble(reader2["AmountYTN"]);
                    counter++;
                }
                reader2.Close();
                connection.Close();
                //
                for (int i = 0; i < TeamTailsCount; i++)
                {
                    //Записываем проигравших
                    LosersArray[LoserCounter] = Convert.ToString(UserLoserTelegramID[i]) + "=(" + Convert.ToDouble(UserLoserAmount[i]) + ")";
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
                double[] UserWinerAmount = new double[TeamTailsCount];
                int[] UserWinerTelegramID = new int[TeamTailsCount];
                int counter = 0;

                //Запись данных победителей в массивы
                connection.Open();
                Sqlcmd.CommandText = "SELECT TelegramID, AmountYTN FROM CurrentGame WHERE Team=1";
                SQLiteDataReader reader4 = Sqlcmd.ExecuteReader();
                while (reader4.Read())
                {
                    UserWinerTelegramID[counter] = Convert.ToInt32(reader4["TelegramID"]);
                    UserWinerAmount[counter] = Convert.ToDouble(reader4["AmountYTN"]);
                    counter++;
                }
                reader4.Close();
                connection.Close();
                //
                for (int i = 0; i < TeamTailsCount; i++)
                {
                    //Считаем потенциальный выигрыш
                    double UserWinerPercent;
                    UserWinerPercent = (UserWinerAmount[i] * 100) / TeamTailsAmount;
                    UserWinerAmount[i] = UserWinerAmount[i] + TeamHeadAmount * (UserWinerPercent / 100);
                    //

                    UniversalLogic(connection, Sqlcmd, UserWinerTelegramID, UserWinerAmount, i);
                    //Записываем победителей
                    WinnersArray[WinnersCounter] = Convert.ToString(UserWinerTelegramID[i])+"=("+Convert.ToDouble(UserWinerAmount[i])+")";
                    WinnersCounter++;
                    //
                }
                //Запись данных проигравших в массивы
                string[] LosersArray = new string[TeamHeadCount];
                double[] UserLoserAmount = new double[TeamHeadCount];
                int[] UserLoserTelegramID = new int[TeamHeadCount];
                counter = 0;
                connection.Open();
                Sqlcmd.CommandText = "SELECT TelegramID, AmountYTN FROM CurrentGame WHERE Team=0";
                SQLiteDataReader reader3 = Sqlcmd.ExecuteReader();
                while (reader3.Read())
                {
                    UserLoserTelegramID[counter] = Convert.ToInt32(reader3["TelegramID"]);
                    UserLoserAmount[counter] = Convert.ToDouble(reader3["AmountYTN"]);
                    counter++;
                }
                reader3.Close();
                connection.Close();
                //
                for (int i = 0; i < TeamHeadCount; i++)
                {
                    //Записываем проигравших
                    LosersArray[LoserCounter] = Convert.ToString(UserLoserTelegramID[i]) + "=(" + Convert.ToDouble(UserLoserAmount[i]) + ")";
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
            connection.Open();
            Sqlcmd.CommandText = "SELECT max(GameID) FROM GameHistory";
            int GameID = Convert.ToInt32(Sqlcmd.ExecuteScalar());
            connection.Close();
            string appendText = DateTime.Now + "  [Log]: Game № " +GameID+
                "\nКоличество участников: "+(TeamHeadCount+TeamTailsCount)
                +"\nКоличество участников команда Ореёл: "+TeamHeadCount+"  Баланс команды: "+TeamHeadAmount
                + "\nКоличество участников команда Решка: " + TeamTailsCount + "  Баланс команды: " + TeamTailsAmount
                + "\nОбщая ставка: "+(TeamHeadAmount+TeamTailsAmount)
                +"\nПобедила команда: "+TeamWinID + Environment.NewLine;

            System.IO.File.AppendAllText("log.txt", appendText);
            Console.WriteLine(appendText);
            //

        }

        private static void UniversalLogic(SQLiteConnection connection, SQLiteCommand Sqlcmd, int[] UserWinerTelegramID, double[] UserWinerAmount, int i)
        {
            //Добавляем выигрыш на счет
            connection.Open();
            Sqlcmd.CommandText = "SELECT WalletIN FROM UserInfo WHERE TelegramID=" + UserWinerTelegramID[i];
            string UserWinerWalletIN = Convert.ToString(Sqlcmd.ExecuteScalar());
            Sqlcmd.CommandText = "SELECT Ballance FROM BallanceCheck WHERE WalletIN=" + "'" + UserWinerWalletIN + "'";
            double UserWinerBallance = Convert.ToDouble(Sqlcmd.ExecuteScalar());
            Sqlcmd.CommandText = @"UPDATE BallanceCheck SET Ballance = :Ballance WHERE WalletIN=" + "'" + UserWinerWalletIN + "'";
            Sqlcmd.Parameters.Add("Ballance", System.Data.DbType.Single).Value = UserWinerBallance + UserWinerAmount[i];
            Sqlcmd.ExecuteNonQuery();
            connection.Close();
            //

        }


        private static void RecordToHistory(SQLiteConnection connection, SQLiteCommand Sqlcmd, string Losers, string Winners, int Team, string AllPlayers)
        {
            Int32 unixTimestamp = (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;

            connection.Open();
            Sqlcmd.CommandText = "SELECT max(GameID) FROM GameHistory";
            int GameID = Convert.ToInt32(Sqlcmd.ExecuteScalar());
            Sqlcmd.CommandText = "INSERT INTO GameHistory VALUES(@GameID, @Losers, @AllPlayers, @Winners, @GameDate, @Team, @NotificationStatus)"; 
            Sqlcmd.Parameters.AddWithValue("@GameID", (GameID+1));
            Sqlcmd.Parameters.AddWithValue("@Losers", Losers);
            Sqlcmd.Parameters.AddWithValue("@Winners", Winners);
            Sqlcmd.Parameters.AddWithValue("@AllPlayers", AllPlayers);
            Sqlcmd.Parameters.AddWithValue("@GameDate", unixTimestamp);
            Sqlcmd.Parameters.AddWithValue("@Team", Team);
            Sqlcmd.Parameters.AddWithValue("@NotificationStatus", 0);
            Sqlcmd.ExecuteNonQuery();

            connection.Close();
        }


    }
}
