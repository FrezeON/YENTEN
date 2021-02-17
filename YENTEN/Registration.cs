using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Text;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using YENTEN.Command.Commands;
using YENTEN.Inline;

namespace YENTEN
{
    class Registration
    {
        private static SQLiteConnection connection;

        public static async void StrartReg (Message message, TelegramBotClient client)
        {
         connection = new SQLiteConnection(@"Data Source=D:\YentLuckyBot\MainDB1.db");
         SQLiteCommand Sqlcmd = connection.CreateCommand();
         string UserWallet = message.Text;
        connection.Open();
                Sqlcmd.CommandText = "SELECT * FROM RawWallets WHERE ROWID= (SELECT min(ROWID) FROM RawWallets)";
                string WalletIN= Convert.ToString(Sqlcmd.ExecuteScalar());
            if (WalletIN == "")
            {
                await client.SendChatActionAsync(message.Chat.Id, ChatAction.Typing);
                await client.SendTextMessageAsync(message.Chat.Id, "К сожаление в базе нет свободных адресов для регистрации.\nОбратитесь за помощью к Оператору @UtkaZapas");
            }
            else
            {
                Sqlcmd.CommandText = "DELETE FROM RawWallets WHERE ROWID=(SELECT min(ROWID) FROM RawWallets)";
                Sqlcmd.ExecuteNonQuery();
                Sqlcmd.CommandText = "INSERT OR IGNORE INTO UserInfo VALUES(@TelegramID, @UserWallet,@WalletIN)";
                Sqlcmd.Parameters.AddWithValue("@TelegramID", message.From.Id);
                Sqlcmd.Parameters.AddWithValue("@UserWallet", UserWallet);
                Sqlcmd.Parameters.AddWithValue("@WalletIN", WalletIN);
                Sqlcmd.ExecuteNonQuery();
                //Вносим в BallanceCheck
                Sqlcmd.CommandText = "INSERT OR IGNORE INTO BallanceCheck VALUES(@WalletIN, @Ballance, @LastIN, @LastAcceted)";
                Sqlcmd.Parameters.AddWithValue("@WalletIN", WalletIN);
                Sqlcmd.Parameters.AddWithValue("@Ballance", 0);
                Sqlcmd.Parameters.AddWithValue("@LastIN", 0);
                Sqlcmd.Parameters.AddWithValue("@LastAcceted", 0);
                Sqlcmd.ExecuteNonQuery();
                //
                connection.Close();

                await client.SendChatActionAsync(message.Chat.Id, ChatAction.Typing);
                await client.SendTextMessageAsync(message.Chat.Id, "Поздравляю, Вы зарегистрированы!");
                MainMenu.SendMAinMenu(client, message);
                Console.WriteLine(DateTime.Now + "  [Log]: "); Console.ForegroundColor = ConsoleColor.Green; Console.Write("НОВЫЙ ПОЛЬЗОВАТЕЛЬ!");
                Console.ForegroundColor = ConsoleColor.White; Console.WriteLine("\nID: " + message.Chat.Id + "\nWallet: " + UserWallet + "\nWalletIN: " + WalletIN);
            }
              
        }
    }
}
