using System;
using System.Collections.Generic;
using System.Text;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using System.Data.SQLite;
using Telegram.Bot.Args;
using Telegram.Bot.Types.Enums;

namespace YENTEN.Command.Commands
{
    public class RegistrationCheck : Command
    {
        private SQLiteConnection connection;

        public override string[] Names { get; set; } = new string[] { "Регистрация", "Reg", "Registration" };
       
        public override async void Execute(Message message, TelegramBotClient client)
        {
            connection = new SQLiteConnection("Data Source=MainDB1.db");
            connection.Open();
           SQLiteCommand Sqlcmd = connection.CreateCommand();
           Sqlcmd.CommandText = "SELECT count(*) FROM UserInfo WHERE TelegramID=" + message.Chat.Id;
           int UserExist = Convert.ToInt32(Sqlcmd.ExecuteScalar());
            connection.Close();
           if (UserExist != 0)
           {
            await client.SendTextMessageAsync(message.Chat.Id, "Вы уже зарегистрированы!");
           }
            else
            {
                await client.SendTextMessageAsync(message.Chat.Id, "Вставьте адрес вашего кошелька." +
                    "\nAдрес кошелька должен быть похож на это: YnNhhuHjnqpk86fdiXd3SXo5DXozEE4Wxv"
                    + "\nВ случае неправильного ввода адреса необходимо обратится к оператору  @UtkaZapas", ParseMode.Default, false, false, 0, replyMarkup: new ForceReplyMarkup { Selective = true });
            }
            
        }

        
    }

}
