using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Data.SQLite;
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
            if (TeamHeadCount != 0 && TeamTailsCount != 0)
            {
                GameLogic(connection, Sqlcmd, TeamHeadCount, TeamTailsCount);
                connection.Open();
                Sqlcmd.CommandText ="DELETE FROM CurrentGame";
                Sqlcmd.ExecuteNonQuery();
                connection.Close();
            }
        }

        private static void GameLogic(SQLiteConnection connection, SQLiteCommand Sqlcmd, int TeamHeadCount, int TeamTailsCount)
        {
            //Парсер Random.org
            string urlAddress = "https://www.random.org/integers/?num=1&min=0&max=1&col=1&base=10&format=plain&rnd=new";
            int TeamWinID =Convert.ToInt32(BallanceCheck.getResponse(urlAddress));
            Console.WriteLine(TeamWinID);
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
            //
            string[] AllArray = new string[(TeamHeadCount + TeamTailsCount)];
            int Count = 0;
            //
            if (TeamWinID == 0)
            {
                string[] WinnersArray = new string[TeamHeadCount]; 
                for (int i =0; i < TeamHeadCount; i++)
                {
                    connection.Open();

                    Sqlcmd.CommandText = "SELECT TelegramID FROM CurrentGame WHERE Team=0";
                    int UserWinerTelegramID = Convert.ToInt32(Sqlcmd.ExecuteScalar());
                    Sqlcmd.CommandText = "SELECT AmountYTN FROM CurrentGame WHERE Team=0";
                    double UserWinerAmount = Convert.ToDouble(Sqlcmd.ExecuteScalar());
                    connection.Close();

                    //Считаем потенциальный выигрыш
                    double UserWinerPercent;
                    UserWinerPercent = (UserWinerAmount * 100) / TeamHeadAmount;
                    UserWinerAmount = UserWinerAmount + TeamTailsAmount * (UserWinerPercent / 100);
                    //

                    //Добавляем выигрыш на счет
                    connection.Open();
                    Sqlcmd.CommandText = "SELECT WalletIN FROM UserInfo WHERE TelegramID=" + UserWinerTelegramID;
                    string UserWinerWalletIN = Convert.ToString(Sqlcmd.ExecuteScalar());
                    Sqlcmd.CommandText = "SELECT Ballance FROM BallanceCheck WHERE WalletIN="+"'" + UserWinerWalletIN + "'";
                    double UserWinerBallance = Convert.ToDouble(Sqlcmd.ExecuteScalar());
                    Sqlcmd.CommandText = @"UPDATE BallanceCheck SET Ballance = :Ballance WHERE WalletIN=" + "'"+UserWinerWalletIN+"'";
                    Sqlcmd.Parameters.Add("Ballance", System.Data.DbType.Single).Value = UserWinerBallance+UserWinerAmount;
                    Sqlcmd.ExecuteNonQuery();
                    connection.Close();
                    //
                    //Записываем победителей
                    WinnersArray[Count] = Convert.ToString(UserWinerTelegramID);
                    Count++;
                    //


                    //Удаляем запись из CurrentGame;
                    connection.Open();
                    Sqlcmd.CommandText ="DELETE FROM CurrentGame WHERE TelegramID=" + UserWinerTelegramID;
                    Sqlcmd.ExecuteNonQuery();
                    connection.Close();
                    //
                    Console.WriteLine(UserWinerTelegramID + "    HW:   "+(TeamTailsAmount * (UserWinerPercent / 100))+"      W" + UserWinerAmount+"  B "+UserWinerBallance+"   W+B  "+(UserWinerBallance+UserWinerAmount));


                }

            }
            else
            {
                string[] WinnersArray = new string[TeamTailsCount];
                for (int i = 0; i < TeamTailsCount; i++)
                {
                    connection.Open();

                    Sqlcmd.CommandText = "SELECT TelegramID FROM CurrentGame WHERE Team=1";
                    int UserWinerTelegramID = Convert.ToInt32(Sqlcmd.ExecuteScalar());
                    Sqlcmd.CommandText = "SELECT AmountYTN FROM CurrentGame WHERE Team=1";
                    double UserWinerAmount = Convert.ToDouble(Sqlcmd.ExecuteScalar());
                    connection.Close();
                    Console.WriteLine("USER winer Amount:  "+UserWinerAmount);
                   //Считаем потенциальный выигрыш
                   double UserWinerPercent;
                    UserWinerPercent = (UserWinerAmount * 100) / TeamTailsAmount;
                    UserWinerAmount = UserWinerAmount + TeamHeadAmount * (UserWinerPercent / 100);
                    //

                    //Добавляем выигрыш на счет
                    connection.Open();
                    Sqlcmd.CommandText = "SELECT WalletIN FROM UserInfo WHERE TelegramID=" + UserWinerTelegramID;
                    string UserWinerWalletIN = Convert.ToString(Sqlcmd.ExecuteScalar());
                    Sqlcmd.CommandText = "SELECT Ballance FROM BallanceCheck WHERE WalletIN=" + "'" + UserWinerWalletIN + "'";
                    double UserWinerBallance = Convert.ToDouble(Sqlcmd.ExecuteScalar());
                    Sqlcmd.CommandText = @"UPDATE BallanceCheck SET Ballance = :Ballance WHERE WalletIN=" + "'" + UserWinerWalletIN + "'";
                    Sqlcmd.Parameters.Add("Ballance", System.Data.DbType.Single).Value = UserWinerBallance + UserWinerAmount;
                    Sqlcmd.ExecuteNonQuery();
                    connection.Close();
                    //

                    //Записываем победителей
                    WinnersArray[Count] = Convert.ToString(UserWinerTelegramID);
                    Count++;
                    //

                    //Удаляем запись из CurrentGame;
                    connection.Open();
                    Sqlcmd.CommandText = "DELETE FROM CurrentGame WHERE TelegramID=" + UserWinerTelegramID;
                    Sqlcmd.ExecuteNonQuery();
                    connection.Close();
                    //
                    Console.WriteLine(UserWinerTelegramID + "    HW:   " + (TeamHeadAmount * (UserWinerPercent / 100)) + "      W" + UserWinerAmount + "  B " + UserWinerBallance + "   W+B  " + (UserWinerBallance + UserWinerAmount));

                }
            }

            //Записываем всех пользователей
            int AllCounter = 0;
            connection.Open();
            Sqlcmd.CommandText = "SELECT TelegramID FROM CurrentGame";
            SQLiteDataReader reader = Sqlcmd.ExecuteReader();
            while (reader.Read())
            {
                AllArray[AllCounter] = reader["TelegramID"].ToString();
                AllCounter++;
            }
            reader.Close();
            connection.Close();
            //

            //Добавляем в лог запись игры
            string appendText = DateTime.Now + "  [Log]: Game № " +
                "\nКоличество участников: "+(TeamHeadCount+TeamTailsCount)
                +"\nКоличество участников команда Ореёл: "+TeamHeadCount+"  Баланс команды: "+TeamHeadAmount
                + "\nКоличество участников команда Решка: " + TeamTailsCount + "  Баланс команды: " + TeamTailsAmount
                + "\nОбщая ставка: "+(TeamHeadAmount+TeamTailsAmount)
                +"\nПобедила команда: "+TeamWinID + Environment.NewLine;

            System.IO.File.AppendAllText(@"D:\YentLuckyBot\log.txt", appendText);
            //


            //
            for (int i =0; i < (TeamHeadCount+TeamTailsCount); i++)
            {
                Console.WriteLine("Пользователь: "+AllArray[i]);
            }
            //
        }


    }
}
