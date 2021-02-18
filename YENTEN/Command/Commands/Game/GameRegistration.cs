using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Text;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace YENTEN.Command.Commands.Game
{
    class GameRegistration : Command
    {
        private static SQLiteConnection connection;

        public override string[] Names { get; set; } = new string[] { "💿Орёл", "📀Решка" };

        public override async void Execute(Message message, TelegramBotClient client)
        {
            //Head(0) - орёл Tails(1)- Решка
            connection = new SQLiteConnection("Data Source=MainDB1.db");
            SQLiteCommand Sqlcmd = connection.CreateCommand();

            //Поиск пользователя в игре
            connection.Open();
            Sqlcmd.CommandText = "SELECT COUNT(*) FROM CurrentGame WHERE TelegramID=" + message.Chat.Id;
            int UserExist = Convert.ToInt32(Sqlcmd.ExecuteScalar());
            Sqlcmd.CommandText = "SELECT AmountYTN FROM CurrentGame WHERE TelegramID=" + message.Chat.Id;
            double AmountInCurrentGame = Convert.ToDouble(Sqlcmd.ExecuteScalar());
            Console.WriteLine(AmountInCurrentGame);
            connection.Close();

            //Получаем ID команды
            int TeamId;
            if (message.Text == "💿Орёл")
            {
                TeamId = 0;
            }
            else
            {
                TeamId = 1;
            }
            //

            if (UserExist == 0)
            {
                
                await client.SendTextMessageAsync(message.Chat.Id, "Введите Вашу ставку в формате 14 или (14.531)", ParseMode.Default, false, false, 0, replyMarkup: new ForceReplyMarkup { Selective = true });
                connection.Open();
                Sqlcmd.CommandText = "INSERT INTO CurrentGame VALUES(@TelegramID, @AmountYTN, @Team)";
                Sqlcmd.Parameters.AddWithValue("@TelegramID", message.From.Id);
                Sqlcmd.Parameters.AddWithValue("@AmountYTN", 0);
                Sqlcmd.Parameters.AddWithValue("@Team", TeamId);
                Sqlcmd.ExecuteNonQuery();
                connection.Close();
            }
            else if (AmountInCurrentGame == 0)
            {

                connection.Open();
                Sqlcmd.CommandText = @"UPDATE CurrentGame SET Team = :Team WHERE TelegramID=" + message.Chat.Id;
                Sqlcmd.Parameters.Add("Team", System.Data.DbType.Single).Value = TeamId;
                Sqlcmd.ExecuteNonQuery();
                connection.Close();
                await client.SendTextMessageAsync(message.Chat.Id, "Введите Вашу ставку в формате 14 или (14.531)", ParseMode.Default, false, false, 0, replyMarkup: new ForceReplyMarkup { Selective = true });
            }
            else
            {
                await client.SendTextMessageAsync(message.Chat.Id, "Вы уже зарегестрированы в этой игре!\n для проверки статуса игры нажмите 🎮Игра");
            }

        }

        public static async void UserReg(Message message, TelegramBotClient client, SQLiteConnection connection)
        {
            //Клавиатура с выбором команды
            var Main = new ReplyKeyboardMarkup();
            Main.Keyboard = new KeyboardButton[][]
            {
                new[]
                {
                new KeyboardButton("🎮Игра"),
                },
                new[]
                {
                    new KeyboardButton("❓Информация"),
                    new KeyboardButton("Меню"),
                    new KeyboardButton("👤Профиль")
                }
            };
            Main.OneTimeKeyboard = true;
            //Клавиатура с выбором команды
            var markupAddGame = new ReplyKeyboardMarkup();
            markupAddGame.Keyboard = new KeyboardButton[][]
            {
                new[]
                {
                new KeyboardButton("💿Орёл"),
                new KeyboardButton("📀Решка")
                },
                new[]
                {
                    new KeyboardButton("Меню")
                }
            };
            markupAddGame.OneTimeKeyboard = true;

            //
            //Поиск пользователя в игре
            connection = new SQLiteConnection("Data Source=MainDB1.db");
            SQLiteCommand Sqlcmd = connection.CreateCommand();
            connection.Open();
            Sqlcmd.CommandText = "SELECT COUNT(*) FROM CurrentGame WHERE TelegramID=" + message.Chat.Id;
            int UserExist = Convert.ToInt32(Sqlcmd.ExecuteScalar());
            Sqlcmd.CommandText = "SELECT WalletIN FROM UserInfo WHERE TelegramID=" + message.Chat.Id;
            string WalletIN = Convert.ToString(Sqlcmd.ExecuteScalar());
            Sqlcmd.CommandText = "SELECT Ballance FROM BallanceCheck WHERE WalletIN=" + "'"+WalletIN+"'";
            double AmountOnBalance = Convert.ToDouble(Sqlcmd.ExecuteScalar());
            Sqlcmd.CommandText = "SELECT AmountYTN FROM CurrentGame WHERE TelegramID=" + message.Chat.Id;
            double AmountInCurrentGame = Convert.ToDouble(Sqlcmd.ExecuteScalar());
            connection.Close();
            //
            if (UserExist == 0 || AmountInCurrentGame ==0)
            {
                double AmountYTN=0;
                try
                {
                    AmountYTN = Convert.ToDouble(message.Text);
                }
                catch (Exception)
                {
                    await client.SendTextMessageAsync(message.Chat.Id, "Введите Вашу ставку в формате 14 или (14.531)", ParseMode.Default, false, false, 0, replyMarkup: new ForceReplyMarkup { Selective = true });
                }
                
                if(AmountOnBalance >= AmountYTN && AmountYTN > 0.03)
                {
                    //Добавляем запись в игру
                    connection.Open();
                    Sqlcmd.CommandText = @"UPDATE CurrentGame SET AmountYTN = :AmountYTN WHERE TelegramID=" + message.Chat.Id;
                    Sqlcmd.Parameters.Add("AmountYTN", System.Data.DbType.Single).Value = AmountYTN;
                    Sqlcmd.ExecuteNonQuery();
                    connection.Close();
                    //Обновляем баланс пользователя   
                    connection.Open();
                    Sqlcmd.CommandText = @"UPDATE BallanceCheck SET Ballance = :Ballance WHERE WalletIN=" +"'"+WalletIN+"'";
                    Sqlcmd.Parameters.Add("Ballance", System.Data.DbType.Single).Value = AmountOnBalance-AmountYTN;
                    Sqlcmd.ExecuteNonQuery();
                    connection.Close();
                    await client.SendTextMessageAsync(message.Chat.Id, "Что дальше?", replyMarkup: Main);
                }
                else
                {
                    await client.SendTextMessageAsync(message.Chat.Id, "На балансе недостаточно средств или Вы ввели неправельное значение (Минимальная ставка 0.03)", replyMarkup: markupAddGame);
                }
            }
            else
            {
                await client.SendTextMessageAsync(message.Chat.Id, "Вы уже зарегестрированы в этой игре!\n для проверки статуса игры нажмите 🎮Игра");
                await client.SendTextMessageAsync(message.Chat.Id, "Что дальше?", replyMarkup: Main);
            }
            
            //
        }
    }
}
