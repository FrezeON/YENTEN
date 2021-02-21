using System;
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
            if (comandText== "Send:")
            {
                client = new TelegramBotClient(Config.Token);
                string Text = Console.ReadLine();
                string queryString = "SELECT count(TelegramID) FROM UserInfo";
                int UserCount = DatabaseLibrary.ExecuteScalarInt(queryString);
                try
                {
                    connection = new SQLiteConnection("Data Source=MainDB1.db;Version=3;New=False;Compress=True;");
                    SQLiteCommand Sqlcmd = connection.CreateCommand();
                    connection.Open();
                    Sqlcmd.CommandText = "SELECT TelegramID FROM UserInfo";
                    SQLiteDataReader reader = Sqlcmd.ExecuteReader();
                    int[] UserID = new int[UserCount];
                    int Counter = 0;
                     while (reader.Read())
                     {
                        Console.WriteLine("PUK");
                        UserID[Counter] = Convert.ToInt32(reader["TelegramID"]);
                        await client.SendTextMessageAsync(UserID[Counter], Text);
                        Counter++;
                     }
                     reader.Close();
                    DatabaseLibrary.ConnectionClose();
                }
                catch(Exception e)
                {
                    string appendText = DateTime.Now + "  [Log]: Ошибка отправки уведомления: " + e;
                    Console.WriteLine(appendText);
                    System.IO.File.AppendAllText("log.txt", appendText);
                }
            }
        }
    }
}
