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

namespace YENTEN.Command.Commands
{
    public class BallanceCheck : Command
    {
        private SQLiteConnection connection;
        public override string[] Names { get; set; } = new string[] { "Ballance", "💸", "Баланс💸" };

        public override async void Execute(Message message, TelegramBotClient client)
        {

            //БД1
            connection = new SQLiteConnection("Data Source=MainDB1.db");
            SQLiteCommand Sqlcmd = connection.CreateCommand();
            string queryString = "SELECT WalletIN FROM UserInfo WHERE TelegramID=" + message.Chat.Id;
            string WalletIn = DatabaseLibrary.ExecuteScalarString(queryString);

            Sqlcmd.CommandText = "SELECT Ballance, LastAcceted, LastIN FROM BallanceCheck WHERE WalletIN='" + WalletIn + "'";
            decimal ballance = 0;
            decimal LastAcceted = 0;
            int LastIN = 0;
            connection.Open();
            SQLiteDataReader reader = Sqlcmd.ExecuteReader();
            while (reader.Read())
            {
                ballance = Convert.ToDecimal(reader["Ballance"]);
                LastAcceted = Convert.ToDecimal(reader["LastAcceted"]);
                LastIN = Convert.ToInt32(reader["LastIN"]);
            }
            reader.Close();
            DatabaseLibrary.ConnectionClose();
            try
            {


                //Парсер
                string urlAddress = "http://ytn.ccore.online/ext/getaddress/" + WalletIn;
                urlAddress = urlAddress.Replace(" ", "");
                string HTML = getResponse(urlAddress);
                //
                //Переворот HTML
                HTML = ReverseString(HTML);
                //
                Console.WriteLine(DateTime.Now + "  [Log]: Пользователь запросил обновление кошелька: " + WalletIn);

                decimal balanceUpdate = ballance;
                int counter = 0;
                while (true)
                {
                    //Условия регуляров
                    Match matchTime = Regex.Match(HTML, "}([01234567890]*?):\"pmatsemit");
                    Match matchAmount = Regex.Match(HTML, "\",([-01234567890.]*?):\\\"tnuoma");
                    string timestamp = ReverseString(matchTime.Groups[1].Value);
                    string Amount = ReverseString(matchAmount.Groups[1].Value);
                    //

                    if (matchTime.Groups[1].Value != "" && Convert.ToInt32(timestamp) > LastIN && Convert.ToDecimal(Amount.Replace('.', ',')) > 0)
                    {

                        LastIN = Convert.ToInt32(timestamp);
                        Console.WriteLine("+" + Convert.ToDecimal(Amount.Replace('.', ',')));
                        balanceUpdate += Convert.ToDecimal(Amount.Replace('.', ','));
                        LastAcceted = Convert.ToDecimal(Amount.Replace('.', ','));
                        counter++;
                    }
                    else if (timestamp == "")
                    {
                        break;
                    }
                    // Удаение из HTML учтенных записей
                    int index = HTML.IndexOf(matchTime.Groups[1].Value + ":\"pmatsemit");
                    if (index != -1)
                    {
                        HTML = HTML.Remove(index, matchTime.Groups[1].Value.Length + 12);
                    }
                    index = HTML.IndexOf(matchAmount.Groups[1].Value + ":\"tnuoma");
                    if (index != -1)
                    {
                        HTML = HTML.Remove(index, matchTime.Groups[1].Value.Length + 9);
                    }
                    // Console.WriteLine(timestamp + "       " + Amount + "        "+balanceUpdate);
                    //
                }
                Console.WriteLine("Для кошелька:  " + WalletIn + "   Было добавленно  " + counter + "  записей!!");
                if (counter != 0)
                {
                    //Запись нового баланса и метки времени в БД
                    DatabaseLibrary.ConnectionOpen();
                    Sqlcmd.CommandText = @"UPDATE BallanceCheck SET Ballance = :Ballance, LastIN = :LastIN, LastAcceted = :LastAcceted WHERE WalletIn='" + WalletIn + "'";
                    Sqlcmd.Parameters.Add("Ballance", System.Data.DbType.Decimal).Value = balanceUpdate;
                    Sqlcmd.Parameters.Add("LastIN", System.Data.DbType.Int32).Value = LastIN;
                    Sqlcmd.Parameters.Add("LastAcceted", System.Data.DbType.Decimal).Value = LastAcceted;
                    Sqlcmd.ExecuteNonQuery();
                    DatabaseLibrary.ConnectionClose();
                }
                //
                //Дата последней транзакции
                DateTime pDate = (new DateTime(1970, 1, 1, 0, 0, 0, 0)).AddSeconds(LastIN).AddHours(3);
                //
                //Сообщение пользователю
                if (pDate == new DateTime(1970, 1, 1, 0, 0, 0, 0))
                {
                    await client.SendTextMessageAsync(message.Chat.Id, "💸Ваш баланс " + balanceUpdate + "YTN"
                    + "\n📆Дата последней записанной транзакции: Отсутствует"
                    + "\n📨Количество монет в последней транзакции: " + LastAcceted);
                }
                else
                {


                    await client.SendTextMessageAsync(message.Chat.Id, "💸Ваш баланс " + balanceUpdate + "YTN"
                        + "\n📆Дата последней записанной транзакции: " + pDate+" МСК"
                        + "\n📨Количество монет в последней транзакции: " + LastAcceted);
                }
                //

                //Клавиатура для профиля
                var markup = new ReplyKeyboardMarkup();
                markup.Keyboard = new KeyboardButton[][]
                {
                new []
                {
                new KeyboardButton("📅История"),
                new KeyboardButton("💸Баланс"),
                new KeyboardButton("📤Вывод с баланса"),
                },
                new[]
                {
                    new KeyboardButton("Меню"),
                }
                };
                markup.OneTimeKeyboard = true;
                await client.SendTextMessageAsync(message.Chat.Id, "Куда дальше?", replyMarkup: markup);
                //
            }
            catch (Exception e)
            {
                await client.SendTextMessageAsync(message.Chat.Id, "Если видите эту ошибку пишите @UtkaZapas, Код ошибки 0x0001");
                Console.WriteLine("код ошибки 0x0001: " + e);
            }

        }
        public static string ReverseString(string s)
        {
            char[] arr = s.ToCharArray();
            Array.Reverse(arr);
            return new string(arr);
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
