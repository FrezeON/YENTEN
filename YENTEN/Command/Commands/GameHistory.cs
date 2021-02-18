using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Text;
using System.Text.RegularExpressions;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace YENTEN.Command.Commands
{
    class GameHistory : Command
    {
        private SQLiteConnection connection;

        public override string[] Names { get; set; } = new string[] { "История", "История игр" };

        public override async void Execute(Message message, TelegramBotClient client)
        {
            Console.WriteLine("Запрос на историю");
            connection = new SQLiteConnection("Data Source=MainDB1.db");
            SQLiteCommand Sqlcmd = connection.CreateCommand();
            int[] UserGameId = new int[5];
            string[] losers = new string[5];
            string[] Winners = new string[5];
            string[] AllPlayers = new string[5];
            int[] GameDate = new int[5];
            int[] UserTeam = new int[5];
            int Counter = 0;
            connection.Open();
            Sqlcmd.CommandText = "SELECT GameID, losers, Winners, AllPlayers, GameDate, Team FROM GameHistory WHERE AllPlayers LIKE @TelegramID";
            Sqlcmd.Parameters.AddWithValue("@TelegramID", ("%"+message.Chat.Id+"%"));
            SQLiteDataReader reader = Sqlcmd.ExecuteReader();
            while (reader.Read())
            {
                UserGameId[Counter] = Convert.ToInt32(reader["GameID"]);
                losers[Counter] = Convert.ToString(reader["losers"]);
                Winners[Counter] = Convert.ToString(reader["Winners"]);
                AllPlayers[Counter] = Convert.ToString(reader["AllPlayers"]);
                GameDate[Counter] = Convert.ToInt32(reader["GameDate"]);
                UserTeam[Counter] = Convert.ToInt32(reader["Team"]);
                Console.WriteLine("Запись:  " + Counter);
                Console.WriteLine("UserGameId: " + UserGameId[Counter]);
                Console.WriteLine("losers: " + losers[Counter]);
                Console.WriteLine("Winners: " + Winners[Counter]);
                Console.WriteLine("AllPlayers: " + AllPlayers[Counter]);
                Console.WriteLine("GameDate: " + GameDate[Counter]);
                Console.WriteLine("UserTeam: " + UserTeam[Counter]);
                Counter++;
            }
            reader.Close();
            connection.Close();

            for (int i = 0; i < 5; i++)
            {
                if (UserGameId[i] != 0)
                {
                    Console.WriteLine("+");
                    Match matchAmount = Regex.Match(AllPlayers[i], Convert.ToString(message.Chat.Id)+"=\\((.*?)\\)");
                    if (Winners[i].Contains(Convert.ToString(message.Chat.Id)))
                    {
                        await client.SendTextMessageAsync(message.Chat.Id, "Номер игры: " + UserGameId[i]
                        + "\nВаш выигрыш: "+ matchAmount.Groups[1].Value);
                    }
                    else
                    {
                        Console.WriteLine("Проиграл");
                        await client.SendTextMessageAsync(message.Chat.Id, "Номер игры: " + UserGameId[i]
                        + "\nВаш проигрыш: " + matchAmount.Groups[1].Value);
                    }
                }
                
            }

        }
    }
}
