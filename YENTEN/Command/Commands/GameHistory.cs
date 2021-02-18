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
            int[] UserGameId = new int[5];
            string[] losers = new string[5];
            string[] Winners = new string[5];
            string[] AllPlayers = new string[5];
            int[] GameDate = new int[5];
            int[] Team = new int[5];
            int Counter = 0;
            connection.Open();
            Sqlcmd.CommandText = "SELECT GameID, losers, Winners, AllPlayers, GameDate, Team FROM GameHistory WHERE AllPlayers LIKE @TelegramID ORDER BY GameID DESC";
            Sqlcmd.Parameters.AddWithValue("@TelegramID", ("%"+message.Chat.Id+"%"));
            SQLiteDataReader reader = Sqlcmd.ExecuteReader();
            while (reader.Read() && Counter < 5)
            {
                UserGameId[Counter] = Convert.ToInt32(reader["GameID"]);
                losers[Counter] = Convert.ToString(reader["losers"]);
                Winners[Counter] = Convert.ToString(reader["Winners"]);
                AllPlayers[Counter] = Convert.ToString(reader["AllPlayers"]);
                GameDate[Counter] = Convert.ToInt32(reader["GameDate"]);
                Team[Counter] = Convert.ToInt32(reader["Team"]);
                Counter++;
            }
            Console.WriteLine(Counter);
            reader.Close();
            connection.Close();
            string[] Teams = new string[] { "💿Орёл", "📀Решка" };
            await client.SendTextMessageAsync(message.Chat.Id, "Вот ваши последние 5 игр:");
            for (int i = 4; i >= 0; i--)
            {
                Console.WriteLine("PUK" + i);
                if (UserGameId[i] != 0)
                {
                    //Timespan в дату
                    DateTime pDate = (new DateTime(1970, 1, 1, 0, 0, 0, 0)).AddSeconds(GameDate[i]);
                    //
                    Match matchAmount = Regex.Match(AllPlayers[i], Convert.ToString(message.Chat.Id) + "=\\((.*?)\\)");
                    if (Winners[i].Contains(Convert.ToString(message.Chat.Id)))
                    {
                        await client.SendTextMessageAsync(message.Chat.Id, "📁Номер игры: " + UserGameId[i]
                        + "\n📅Дата игры: " + pDate
                        + "\n🛡Победила команда: " + Teams[Team[i]]
                        + "\n💰Ваш выигрыш: " + matchAmount.Groups[1].Value);
                    }
                    else
                    {
                        await client.SendTextMessageAsync(message.Chat.Id, "📁Номер игры: " + UserGameId[i]
                        + "\n📅Дата игры: " + pDate
                        + "\n🛡Победила команда: " + Teams[Team[i]]
                        + "\n💰Ваш проигрыш: " + matchAmount.Groups[1].Value);
                    }

                }
                else if (i == 0 && UserGameId[4] ==0)
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
                new KeyboardButton("💸Баланс")
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
