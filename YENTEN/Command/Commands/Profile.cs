using System;
using System.Collections.Generic;
using System.Text;
using Telegram.Bot;
using Telegram.Bot.Types;
using System.Data.SQLite;
using Telegram.Bot.Types.ReplyMarkups;

namespace YENTEN.Command.Commands
{
    public class Profile : Command
    {
        private SQLiteConnection connection;

        public override string[] Names { get; set; } = new string[] { "Profile", "👤", "Профиль👤" };

        public override async void Execute(Message message, TelegramBotClient client)
        {
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
            await client.SendTextMessageAsync(message.Chat.Id, "Ваш профиль", replyMarkup: markup);
            //


            connection = new SQLiteConnection("Data Source=MainDB1.db");
            SQLiteCommand Sqlcmd = connection.CreateCommand();
            connection.Open();
            Sqlcmd.CommandText = "SELECT rowid FROM UserInfo WHERE TelegramID=" + message.Chat.Id;
            int rowid = Convert.ToInt32(Sqlcmd.ExecuteScalar());
            Sqlcmd.CommandText = "SELECT UserWallet FROM UserInfo WHERE rowid=" + rowid;
            string UserWallet = Convert.ToString(Sqlcmd.ExecuteScalar());
            Sqlcmd.CommandText = "SELECT WalletIN FROM UserInfo WHERE rowid=" + rowid;
            string WalletIN = Convert.ToString(Sqlcmd.ExecuteScalar());
            connection.Close();
            await client.SendTextMessageAsync(message.Chat.Id, "👤Ваш ID: " + message.Chat.Id + "\n💸Кошелек для вывода: " + UserWallet + "\n💸Кошелек для пополнения баланса: " + WalletIN);
        }
    }
}
