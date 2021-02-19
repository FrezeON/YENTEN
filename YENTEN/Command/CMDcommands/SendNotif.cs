﻿using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Text;
using Telegram.Bot;
namespace YENTEN.Command.CMDcommands
{
    class SendNotif : CMDcommand
    {
        public override string[] Names { get; set; } = new string[] { "Send:"};
        private static TelegramBotClient client;
        private static SQLiteConnection connection;
        public override async void Execute(string comandText)
        {
            client = new TelegramBotClient(Config.Token);
            connection = new SQLiteConnection("Data Source=MainDB1.db");
            if (comandText== "Send:")
            {
                string Text = Console.ReadLine();
                SQLiteCommand Sqlcmd = connection.CreateCommand();
                connection.Open();
                Sqlcmd.CommandText = "SELECT count(TelegramID) FROM UserInfo";
                int UserCount = Convert.ToInt32(Sqlcmd.ExecuteScalar());
                Sqlcmd.CommandText = "SELECT TelegramID FROM UserInfo";
                SQLiteDataReader reader = Sqlcmd.ExecuteReader();
                int[] UserID = new int[UserCount];
                int Counter = 0;
                while (reader.Read())
                {
                    UserID[Counter] = Convert.ToInt32(reader["TelegramID"]);
                    await client.SendTextMessageAsync(UserID[Counter], Text);
                    Counter++;
                }
                reader.Close();
                connection.Close();
            }
        }
    }
}
