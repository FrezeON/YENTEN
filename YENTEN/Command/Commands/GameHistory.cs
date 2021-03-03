using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Text;
using System.Text.RegularExpressions;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace YENTEN.Command.Commands
{
    class GameHistory : Command
    {
        private SQLiteConnection connection;

        public override string[] Names { get; set; } = new string[] { "История", "История игр" };

        public override async void Execute(Message message, TelegramBotClient client)
        {
            connection = new SQLiteConnection("Data Source=MainDB1.db");
            SQLiteCommand Sqlcmd = connection.CreateCommand();
            int[] GameId = new int[5];
            string[] losers = new string[5];
            string[] Winners = new string[5];
            string[] AllPlayers = new string[5];
            string[] GameDate = new string[5];
            int[] Team = new int[5];
            int Counter = 0;
            connection.Open();
            Sqlcmd.CommandText = "SELECT GameID, losers, Winners, AllPlayers, GameDate, Team FROM GameHistory WHERE AllPlayers LIKE @TelegramID ORDER BY GameID DESC";
            Sqlcmd.Parameters.AddWithValue("@TelegramID", ("%"+message.Chat.Id+"%"));
            SQLiteDataReader reader = Sqlcmd.ExecuteReader();
            try
            {

                while (reader.Read() && Counter < 5)
                {
                    GameId[Counter] = Convert.ToInt32(reader["GameID"]);
                    losers[Counter] = Convert.ToString(reader["losers"]);
                    Winners[Counter] = Convert.ToString(reader["Winners"]);
                    AllPlayers[Counter] = Convert.ToString(reader["AllPlayers"]);
                    GameDate[Counter] = Convert.ToString(reader["GameDate"]);
                    Team[Counter] = Convert.ToInt32(reader["Team"]);
                    Counter++;
                }
            }catch(Exception e)
            {
                string appendText = DateTime.Now + "  [Log]: GameHistrory READER ошибка: " + e;
                Console.WriteLine(appendText);
                System.IO.File.AppendAllText("log.txt", appendText);
            }
            finally
            {
                reader.Close();
                DatabaseLibrary.ConnectionClose();
            }
            reader.Close();
            connection.Close();
            string[] Teams = new string[] { "💿Орёл", "📀Решка" };
            await client.SendTextMessageAsync(message.Chat.Id, "Вот ваши последние 5 игр:");
            for (int i = 4; i >= 0; i--)
            {
                if (GameId[i] != 0)
                {
                    //
                    if (Winners[i].Contains(Convert.ToString(message.Chat.Id)))
                    {
                        Match matchAmount = Regex.Match(AllPlayers[i], Convert.ToString(message.Chat.Id) + "=\\((.*?):(.*?)\\)");
                        await client.SendTextMessageAsync(message.Chat.Id, "📁Номер игры: " + GameId[i]
                        + "\n📅Дата игры: " + GameDate[i]
                        + "\n🛡Победила команда: " + Teams[Team[i]]
                        + "\n💰Ваша ставка: " + matchAmount.Groups[2].Value+"YTN"
                        + "\n💎Ваш выигрыш: " + matchAmount.Groups[1].Value+"YTN");
                    }
                    else
                    {
                        Match matchAmountLoser = Regex.Match(AllPlayers[i], Convert.ToString(message.Chat.Id) + "=\\((.*?)\\)");
                        await client.SendTextMessageAsync(message.Chat.Id, "📁Номер игры: " + GameId[i]
                        + "\n📅Дата игры: " + GameDate[i]
                        + "\n🛡Победила команда: " + Teams[Team[i]]
                        + "\n💔Ваш проигрыш: " + matchAmountLoser.Groups[1].Value+"YTN");
                    }

                }
                else if (i == 0 && GameId[4] ==0)
                {
                    await client.SendTextMessageAsync(message.Chat.Id,"В вашей истории пока нет завершенных игр");
                }
                
            }
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
    }
}
