using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Text;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace YENTEN.Command.Commands
{
    class WithdrawFunds : Command
    {
        public override string[] Names { get; set; } = new string[] { "Вывод", "Снять с баланса" };

        public override async void Execute(Message message, TelegramBotClient client)
        {
            SQLiteConnection connection;
            connection = new SQLiteConnection("Data Source=MainDB1.db");
            SQLiteCommand Sqlcmd = connection.CreateCommand();

            connection.Open();
            Sqlcmd.CommandText = "SELECT UserWallet FROM UserInfo WHERE TelegramID=" + message.Chat.Id;
            string UserWallet = Convert.ToString(Sqlcmd.ExecuteScalar());
            Sqlcmd.CommandText = "SELECT WalletIN FROM UserInfo WHERE TelegramID=" + message.Chat.Id;
            string WalletIn = Convert.ToString(Sqlcmd.ExecuteScalar());
            Sqlcmd.CommandText = "SELECT Ballance FROM BallanceCheck WHERE WalletIN=" + "'" + WalletIn + "'";
            decimal Ballance = Convert.ToDecimal(Sqlcmd.ExecuteScalar());
            connection.Close();

            await client.SendTextMessageAsync(message.Chat.Id, "Ваш баланс: " + Ballance+"YTN"
                + "\nЕсли вы хотите вывести ваши средства на кошелек:" +
                "\n" + UserWallet +
                "\nОтветьте на седующее сообщение с суммой которую вы хотите вывести в формате" +
                "\n14 или 14.41311"
                + "\n❗️На данный момент минимальная сумма вывода составляет 1YTN❗️"
                +"\nКомиссия на вывод составляет - 3%"
                + "\nЕсли вы хотите сменить кошелек для вывода обратитесь к оператору @UtkaZapas");

            await client.SendTextMessageAsync(message.Chat.Id, "Подтверждаю", ParseMode.Default, false, false, 0, replyMarkup: new ForceReplyMarkup { Selective = true });
        }

        public static async void WithdrawFundsApprowed(Message message, TelegramBotClient client)
        {
            SQLiteConnection connection;


            try
            {
                connection = new SQLiteConnection("Data Source=MainDB1.db");
                SQLiteCommand Sqlcmd = connection.CreateCommand();
                string UserMessage = message.Text.Replace(".", ",");
                decimal AmountWinthdraw = Convert.ToDecimal(UserMessage);
                connection.Open();
                Sqlcmd.CommandText = "SELECT WalletIN FROM UserInfo WHERE TelegramID=" + message.Chat.Id;
                string WalletIn = Convert.ToString(Sqlcmd.ExecuteScalar());
                Sqlcmd.CommandText = "SELECT UserWallet FROM UserInfo WHERE TelegramID=" + message.Chat.Id;
                string UserWallet = Convert.ToString(Sqlcmd.ExecuteScalar());
                Sqlcmd.CommandText = "SELECT Ballance FROM BallanceCheck WHERE WalletIN=" + "'" + WalletIn + "'";
                decimal Ballance = Convert.ToDecimal(Sqlcmd.ExecuteScalar());
                connection.Close();

                if (AmountWinthdraw <= Ballance && AmountWinthdraw >= 1)
                {
                    connection.Open();
                    decimal AmountWinthdrawMinusСommission = AmountWinthdraw * 97 / 100;
                    decimal Сommission = AmountWinthdraw - AmountWinthdrawMinusСommission;
                    //Создаем заявку
                    Sqlcmd.CommandText = "INSERT INTO WithdrawFunds VALUES(@TelegramID, @UserWallet, @AmountWinthdraw, @Сommission)";
                    Sqlcmd.Parameters.AddWithValue("@TelegramID", message.From.Id);
                    Sqlcmd.Parameters.AddWithValue("@UserWallet", UserWallet);
                    Sqlcmd.Parameters.AddWithValue("@AmountWinthdraw", AmountWinthdrawMinusСommission);
                    Sqlcmd.Parameters.AddWithValue("@Сommission", Сommission);
                    Sqlcmd.ExecuteNonQuery();
                    //Отправляем пользователю на счет
                    YentenCalls.SendToAddress(UserWallet, AmountWinthdrawMinusСommission);
                    //
                    //Обновляем баланс
                    Sqlcmd.CommandText = @"UPDATE BallanceCheck SET Ballance = :Ballance WHERE WalletIN=" + "'" + WalletIn + "'";
                    Sqlcmd.Parameters.Add("Ballance", System.Data.DbType.Decimal).Value = Ballance - AmountWinthdraw;
                    Sqlcmd.ExecuteNonQuery();

                    connection.Close();

                    await client.SendTextMessageAsync(message.Chat.Id, "✅Ваша заявка на вывод средств в размере:\n" + AmountWinthdraw + "YTN"
                        + "\nС вычетом комисии вы получите ~" + AmountWinthdrawMinusСommission + "YTN"
                        + "\nПодтвержденна!");
                    string log = DateTime.Now + "  [Log]: Вывод с баланса: \nCумма на кошелек: " + AmountWinthdraw + "\nКомиссия: " + Сommission + "\nПрищло на кошелек: " + AmountWinthdrawMinusСommission;
                    System.IO.File.AppendAllText("log.txt", log);
                    Console.WriteLine(log);
                }
                else
                {
                    await client.SendTextMessageAsync(message.Chat.Id, "❌На вашем балансе недостаточно средств или сумма меньше минимально допустимой!\nПовторите заявку с правильной суммой");
                }

            }

            catch (Exception)
            {
                await client.SendTextMessageAsync(message.Chat.Id, "❌При создании заявки произошла ошибка" +
                    "\nПопробуйте создать заявку снова либо обратитесь к оператору");
               System.IO.File.AppendAllText("log.txt", "\n"+DateTime.Now + "[Log]: ОШИБКА СОЗДАНИЯ ЗАЯВКИ! TelegramID=" + message.Chat.Id + " Пользователь написал: " + message.Text);
               Console.ForegroundColor = ConsoleColor.Red; Console.WriteLine(DateTime.Now + "[Log]: ОШИБКА СОЗДАНИЯ ЗАЯВКИ! TelegramID=" + message.Chat.Id + " Пользователь написал: " + message.Text); Console.ForegroundColor = ConsoleColor.White;
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
            await client.SendTextMessageAsync(message.Chat.Id, "Ваш профиль", replyMarkup: markup);
            //
        }

    }
}    