using System;
using System.Collections.Generic;
using System.Text;
using Telegram.Bot;
using Telegram.Bot.Types;
using System.Data.SQLite;
using System.Net;
using System.IO;
using System.Text.RegularExpressions;
using Telegram.Bot.Types.ReplyMarkups;
using BitcoinLib;
using BitcoinLib.Services.Coins.Cryptocoin;
using YENTEN.Inline;

namespace YENTEN.Command.Commands
{
    public class BallanceCheck : Command
    {
        private SQLiteConnection connection;
        public override string[] Names { get; set; } = new string[] { "Ballance", "💸", "Баланс💸" };

        public override async void Execute(Message message, TelegramBotClient client)
        {
            //БД1
            string queryString = "SELECT WalletIN FROM UserInfo WHERE TelegramID=" + message.Chat.Id;
            string WalletIn = DatabaseLibrary.ExecuteScalarString(queryString);
            queryString = "SELECT LastBallanceCheck FROM BallanceCheck WHERE WalletIN='" + WalletIn + "'";
            decimal LastBallanceCheck = DatabaseLibrary.ExecuteScalarDecimal(queryString);
            queryString = "SELECT Ballance FROM BallanceCheck WHERE WalletIN='" + WalletIn + "'";
            decimal ballance = DatabaseLibrary.ExecuteScalarDecimal(queryString);
            try
            {
                //Получаем информацию о баланасе
                decimal UserBallanceAllTime = YentenCalls.GetAdressInputTransaction(WalletIn);
                decimal DifferenceInBalance = UserBallanceAllTime - LastBallanceCheck;
                //
                if ((UserBallanceAllTime- 0.00000001m)> LastBallanceCheck)
                {
                    Console.WriteLine("Для кошелька:  " + WalletIn + "   Было добавленно  " + DifferenceInBalance + "YTN");
                    //Запись нового баланса и метки времени в БД
                    connection = new SQLiteConnection("Data Source=MainDB1.db");
                    SQLiteCommand Sqlcmd = connection.CreateCommand();
                    connection.Open();
                    Sqlcmd.CommandText = @"UPDATE BallanceCheck SET Ballance =:Ballance, LastBallanceCheck =:LastBallanceCheck WHERE WalletIn='" + WalletIn +"'";
                    Sqlcmd.Parameters.Add("Ballance", System.Data.DbType.Decimal).Value = (ballance + DifferenceInBalance);
                    Sqlcmd.Parameters.Add("LastBallanceCheck", System.Data.DbType.Decimal).Value = UserBallanceAllTime;
                    Sqlcmd.ExecuteNonQuery();
                    connection.Close();
                    //Отправляем уведомление 
                    await client.SendTextMessageAsync(message.Chat.Id, "💸Ваш баланс " + (ballance + DifferenceInBalance) + "YTN"
                    + "\n📨Количество монет в последней транзакции: " + DifferenceInBalance);
                    //
                }
                else
                {
                    //Отправляем уведомление 
                    await client.SendTextMessageAsync(message.Chat.Id, "💸Ваш баланс " + ballance + "YTN"
                        +"\nНовые транзакции не обнаружены");
                    //
                }

                //Клавиатура для профиля
                KeyBoards.SendPtofileKeyBoardAsync(client, message);
                //
            }
            catch (Exception e)
            {
                await client.SendTextMessageAsync(message.Chat.Id, "Если видите эту ошибку пишите @UtkaZapas, Код ошибки 0x0001");
                Console.WriteLine("код ошибки 0x0001: " + e);
            }

        }
        public static string getResponse(string uri)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();

            Stream receiveStream = response.GetResponseStream();
            StreamReader readStream = null;

            if (String.IsNullOrWhiteSpace(response.CharacterSet))
            {
                readStream = new StreamReader(receiveStream);
            }
            else
            {
                readStream = new StreamReader(receiveStream, Encoding.GetEncoding(response.CharacterSet));
            }
            string data = readStream.ReadToEnd();
            response.Close();
            readStream.Close();
            return data;
        }

    }
}
