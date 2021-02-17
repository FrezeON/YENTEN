using System;
using System.Collections.Generic;
using System.Text;
using Telegram.Bot;
using Telegram.Bot.Types;
using System.Data.SQLite;
using System.Net;
using System.IO;
using System.Text.RegularExpressions;

namespace YENTEN.Command.Commands
{
    public class BallanceCheck : Command
    {
        private SQLiteConnection connection;
        public override string[] Names { get; set; } = new string[] { "Ballance", "💸", "Баланс💸" };

        public override async void Execute(Message message, TelegramBotClient client)
        {
            
            //БД1
            connection = new SQLiteConnection(@"Data Source=D:\YentLuckyBot\MainDB1.db");
            SQLiteCommand Sqlcmd = connection.CreateCommand();
            connection.Open();
            Sqlcmd.CommandText = "SELECT rowid FROM UserInfo WHERE TelegramID = " + message.Chat.Id;
            int rowid = Convert.ToInt32(Sqlcmd.ExecuteScalar());
            Sqlcmd.CommandText = "SELECT WalletIN FROM UserInfo WHERE rowid=" + rowid;
            string WalletIn = Convert.ToString(Sqlcmd.ExecuteScalar());
            Sqlcmd.CommandText = "SELECT rowid FROM BallanceCheck WHERE WalletIN = '"+WalletIn+"'";
            int rowidFromBallance = Convert.ToInt32(Sqlcmd.ExecuteScalar());
            Sqlcmd.CommandText = "SELECT Ballance FROM BallanceCheck WHERE rowid=" + rowidFromBallance;
            double ballance = Convert.ToDouble(Sqlcmd.ExecuteScalar());
            Sqlcmd.CommandText = "SELECT LastAcceted FROM BallanceCheck WHERE rowid=" + rowidFromBallance;
            double LastAcceted = Convert.ToDouble(Sqlcmd.ExecuteScalar());
            Sqlcmd.CommandText = "SELECT LastIN FROM BallanceCheck WHERE rowid=" + rowidFromBallance;
            int LastIN = Convert.ToInt32(Sqlcmd.ExecuteScalar());
            connection.Close();
            //
            //Парсер
            string urlAddress = "http://ytn.ccore.online/ext/getaddress/"+WalletIn;
            string HTML = getResponse(urlAddress);
            //
            //Переворот HTML
            HTML = ReverseString(HTML);
            //Console.WriteLine(HTML);
            //
            Console.WriteLine(DateTime.Now + "  [Log]: Пользователь запросил обновление кошелька: "+WalletIn);

            double balanceUpdate =ballance;
            int counter = 0;
            while (true)
            {
                //Условия регуляров
                Match matchTime = Regex.Match(HTML, "}([01234567890]*?):\"pmatsemit");
                Match matchAmount = Regex.Match(HTML, "\",([-01234567890.]*?):\\\"tnuoma");
                string timestamp = ReverseString(matchTime.Groups[1].Value);
                string Amount = ReverseString(matchAmount.Groups[1].Value);
                //
                if (matchTime.Groups[1].Value != "" && Convert.ToInt32(timestamp) > LastIN && Convert.ToDouble(Amount.Replace('.',',')) > 0)
                {
                    LastIN = Convert.ToInt32(timestamp);
                    Console.WriteLine("+"+ Convert.ToDouble(Amount.Replace('.', ',')));
                    balanceUpdate += Convert.ToDouble(Amount.Replace('.', ','));
                    LastAcceted = Convert.ToDouble(Amount.Replace('.', ','));
                    counter++;
                }
                else if(timestamp == "")
                {
                    break;
                }
                // Удаение из HTML учтенных записей
                int index = HTML.IndexOf(matchTime.Groups[1].Value+":\"pmatsemit");
                if(index != -1)
                {
                    HTML = HTML.Remove(index, matchTime.Groups[1].Value.Length+12);
                }
                index = HTML.IndexOf(matchAmount.Groups[1].Value+":\"tnuoma");
                if (index != -1)
                {
                    HTML = HTML.Remove(index, matchTime.Groups[1].Value.Length+9);
                }
              // Console.WriteLine(timestamp + "       " + Amount + "        "+balanceUpdate);
                //
            }
            Console.WriteLine("Для кошелька:  " + WalletIn + "   Было добавленно  " + counter + "  записей!!");
            //Запись нового баланса и метки времени в БД
            connection.Open();
            Sqlcmd.CommandText = @"UPDATE BallanceCheck SET Ballance = :Ballance, LastIN = :LastIN, LastAcceted = :LastAcceted WHERE rowid=" + rowidFromBallance;
            Sqlcmd.Parameters.Add("Ballance", System.Data.DbType.Single).Value = balanceUpdate;
            Sqlcmd.Parameters.Add("LastIN", System.Data.DbType.Int32).Value = LastIN;
            Sqlcmd.Parameters.Add("LastAcceted", System.Data.DbType.Single).Value = LastAcceted;
            Sqlcmd.ExecuteNonQuery();
            connection.Close();
            //
            //Дата последней транзакции
            DateTime pDate = (new DateTime(1970, 1, 1, 0, 0, 0, 0)).AddSeconds(LastIN);
            //
            //Сообщение пользователю

            await client.SendTextMessageAsync(message.Chat.Id, "💸Ваш баланс "+balanceUpdate+"YTN"
                + "\n📆Дата последней записанной транзакции: "+pDate
                + "📨Количество монет в последней транзакции: "+ LastAcceted);

            //


        }

        public static string ReverseString(string s)
        {
            char[] arr = s.ToCharArray();
            Array.Reverse(arr);
            return new string(arr);
        }

        public static string getResponse(string uri)
        {
            StringBuilder sb = new StringBuilder();
            byte[] buf = new byte[8192];
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream resStream = response.GetResponseStream();
            int count = 0;
            do
            {
                count = resStream.Read(buf, 0, buf.Length);
                if (count != 0)
                {
                    sb.Append(Encoding.Default.GetString(buf, 0, count));
                }
            }
            while (count > 0);
            return sb.ToString();
        }
    }
}
