using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Text;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using YENTEN.Command.Commands;
using YENTEN.Inline;

namespace YENTEN
{
    class Registration
    {
        private static SQLiteConnection connection;

        public static async void StrartReg (Message message, TelegramBotClient client)
        {
            string queryString = "SELECT count(*) FROM UserInfo WHERE TelegramID=" + message.Chat.Id;
            int check = DatabaseLibrary.ExecuteScalarInt(queryString);
            if(check == 0)
            {
                connection = new SQLiteConnection("Data Source=MainDB1.db");
                SQLiteCommand Sqlcmd = connection.CreateCommand();
                string UserWallet = message.Text;
                //Генерируем новый адрес
                string WalletIN = YentenCalls.GetNewAddress();
                //
                queryString = "INSERT INTO UserInfo VALUES('" + message.From.Id + "','" + UserWallet + "','" + WalletIN + "')";
                DatabaseLibrary.ExecuteNonQuery(queryString);
                //Вносим в BallanceCheck
                queryString = "INSERT OR IGNORE INTO BallanceCheck VALUES('" + WalletIN + "','" + 0 + "','" + 0 + "')";
                DatabaseLibrary.ExecuteNonQuery(queryString);
                //
                await client.SendChatActionAsync(message.Chat.Id, ChatAction.Typing);
                await client.SendTextMessageAsync(message.Chat.Id, "Поздравляю, Вы зарегистрированы!");
                MainMenu.SendMAinMenu(client, message);
                Console.WriteLine(DateTime.Now + "  [Log]: "); Console.ForegroundColor = ConsoleColor.Green; Console.Write("НОВЫЙ ПОЛЬЗОВАТЕЛЬ!");
                Console.ForegroundColor = ConsoleColor.White; Console.WriteLine("\nID: " + message.Chat.Id + "\nWallet: " + UserWallet + "\nWalletIN: " + WalletIN);
                System.IO.File.AppendAllText("log.txt", DateTime.Now + "  [Log]: НОВЫЙ ПОЛЬЗОВАТЕЛЬ!" +
                    "\nID: " + message.Chat.Id +
                    "\nWallet: " + UserWallet +
                    "\nWalletIN: " + WalletIN);

            }
            else
            {
                await client.SendTextMessageAsync(message.Chat.Id, "Вы  уже зарегистрированы!");
            }
        }
    }
}
