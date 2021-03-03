using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Text;
using System.Text.RegularExpressions;
using System.Timers;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace YENTEN.Command.Commands
{
    class NotifyUserAfterGame : Command
    {
        private static TelegramBotClient client;
        private static SQLiteConnection connection;

        public override string[] Names { get; set; }

        public override void Execute(Message message, TelegramBotClient client)
        {
            

        }

        public static async void SenNotification(Object source, ElapsedEventArgs e)
        {
            client = new TelegramBotClient(Config.Token);
            connection = new SQLiteConnection("Data Source=MainDB1.db");
            SQLiteCommand Sqlcmd = connection.CreateCommand();
            connection.Open();
            Sqlcmd.CommandText = "SELECT max(GameID) FROM GameHistory";
            int MaxGameID = Convert.ToInt32(Sqlcmd.ExecuteScalar());
            Sqlcmd.CommandText = "SELECT NotificationStatus FROM GameHistory WHERE GameID="+MaxGameID;
            int NotificationStatus = Convert.ToInt32(Sqlcmd.ExecuteScalar());
            connection.Close();
            if(NotificationStatus == 0)
            {
                connection.Open();
                Sqlcmd.CommandText = "SELECT WInners FROM GameHistory WHERE GameID=" + MaxGameID;
                string Winners = Convert.ToString(Sqlcmd.ExecuteScalar());
                Sqlcmd.CommandText = "SELECT AllPlayers FROM GameHistory WHERE GameID="+MaxGameID;
                string AllPlayers = Convert.ToString(Sqlcmd.ExecuteScalar());
                connection.Close();
                AllPlayers = ")," + AllPlayers;
                Match matchAmount = Regex.Match(AllPlayers, "\\),(.*?)=\\(");
                for(; ; )
                {
                    
                    if(matchAmount.Groups[1].Value != "")
                    {
                        try
                        {
                            if (Winners.Contains(matchAmount.Groups[1].Value))
                            {
                                Match matchAmount2 = Regex.Match(AllPlayers, Convert.ToString(matchAmount.Groups[1].Value) + "=\\((.*?):(.*?)\\)");
                                await client.SendTextMessageAsync(matchAmount.Groups[1].Value, "❗️Игра №:" + MaxGameID + " завершилась!"
                                    + "\n💰Ваша ставка: " + matchAmount2.Groups[2].Value+"YTN"
                                    + "\n💎Ваш выигрыш: " + matchAmount2.Groups[1].Value+"YTN");
                            }
                            else
                            {
                                Match matchAmountLoser = Regex.Match(AllPlayers, Convert.ToString(matchAmount.Groups[1].Value) + "=\\((.*?)\\)");
                                await client.SendTextMessageAsync(matchAmount.Groups[1].Value, "❗️Игра №:" + MaxGameID + " завершилась!"
                                + "\n💰Ваш проигрыш: " + matchAmountLoser.Groups[1].Value+"YTN");
                            }

                        }
                        catch(Exception)
                        {
                            System.IO.File.AppendAllText("log.txt", DateTime.Now + "  [Log]: Не получилось отправить уведомление. Номер игры="+MaxGameID+"  ChatID="+ matchAmount.Groups[1].Value);
                            Console.WriteLine(DateTime.Now + "  [Log]: Не получилось отправить уведомление. Номер игры=" + MaxGameID + "  ChatID=" + matchAmount);
                        }

                    }
                    else
                    {
                        break;
                    }
                    matchAmount=matchAmount.NextMatch();
                }
                connection.Open();
                Sqlcmd.CommandText = @"UPDATE GameHistory SET NotificationStatus = :NotificationStatus WHERE GameID=" + MaxGameID;
                Sqlcmd.Parameters.Add("NotificationStatus", System.Data.DbType.Int32).Value = 1;
                Sqlcmd.ExecuteNonQuery();
                connection.Close();
            }
        }


    }
}
