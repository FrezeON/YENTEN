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
    public class EnterTheGame : Command
    {
        private static SQLiteConnection connection;
        public override string[] Names { get; set; } = new string[] { "Игра", "Войти в игру" };

        public override void Execute(Message message, TelegramBotClient client)
        {

            connection = new SQLiteConnection("Data Source=MainDB1.db");
            SQLiteCommand Sqlcmd = connection.CreateCommand();
            //Подсчет суммы по командам     Количество игроков в командах     Head - орёл Tails- Решка
            double TeamHeadAmount =0;
            double TeamTailsAmount =0;
            int TeamHeadCout = 0;
            int TeamTailsCout = 0;
            connection.Open();
            Sqlcmd.CommandText = "SELECT max(rowid) FROM CurrentGame";
            int maxRowID = Convert.ToInt32(Sqlcmd.ExecuteScalar());
            Sqlcmd.CommandText = "SELECT min(rowid) FROM CurrentGame";
            int minRowID = Convert.ToInt32(Sqlcmd.ExecuteScalar());
            for (int i = minRowID; i <= maxRowID; i++)
            {
                Sqlcmd.CommandText = "SELECT AmountYTN FROM CurrentGame WHERE rowid="+i;
                double Amount = Convert.ToDouble(Sqlcmd.ExecuteScalar());
                Sqlcmd.CommandText = "SELECT Team FROM CurrentGame WHERE rowid=" + i;
                int TeamNumber = Convert.ToInt32(Sqlcmd.ExecuteScalar());
                Console.WriteLine(TeamNumber + "      " + Amount);
                if(TeamNumber == 0)
                {
                    TeamHeadAmount += Amount;
                    TeamHeadCout++;
                }
                else
                {
                    TeamTailsAmount += Amount;
                    TeamTailsCout++;
                }
            }
            connection.Close();
            //

            //Процентное соотношение по балансу
            double TeamHeadPercent = Math.Round((TeamHeadAmount*100) / (TeamHeadAmount + TeamTailsAmount), 2);
            double TeamTailsPercent = Math.Round(100 - TeamHeadPercent,2);
            //       
            //Поиск пользователя в игре
            connection.Open();
            Sqlcmd.CommandText = "SELECT COUNT(*) FROM CurrentGame WHERE TelegramID=" + message.Chat.Id;
            int UserExist = Convert.ToInt32(Sqlcmd.ExecuteScalar());
            Sqlcmd.CommandText = "SELECT AmountYTN FROM CurrentGame WHERE TelegramID=" + message.Chat.Id;
            double AmountInCurrentGame = Convert.ToDouble(Sqlcmd.ExecuteScalar());
            connection.Close();
            //
            if(UserExist == 0 || AmountInCurrentGame ==0)
            {
                UserDoesNotExistAction(message, client, connection, Sqlcmd, TeamHeadAmount, TeamTailsAmount, TeamHeadCout, TeamTailsCout, TeamHeadPercent, TeamTailsPercent);
            }
            else
            {
                UserExistAction(message, client, TeamHeadCout, TeamTailsCout, TeamHeadAmount, TeamTailsAmount, TeamHeadPercent, TeamTailsPercent, Sqlcmd);
            }
            
        }
        public async void UserExistAction(Message message, TelegramBotClient client, int TeamHeadCout, int TeamTailsCout, double TeamHeadAmount,
            double TeamTailsAmount, double TeamHeadPercent, double TeamTailsPercent, SQLiteCommand Sqlcmd)
        {
            //Клавиатура с выбором команды
            var markup = new ReplyKeyboardMarkup();
            markup.Keyboard = new KeyboardButton[][]
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
            markup.OneTimeKeyboard = true;
            //
            //Берем данные пользователя
            connection.Open();
            Sqlcmd.CommandText = "SELECT AmountYTN FROM CurrentGame WHERE TelegramID=" + message.Chat.Id;
            double UserAmount = Convert.ToDouble(Sqlcmd.ExecuteScalar());
            Sqlcmd.CommandText = "SELECT Team FROM CurrentGame WHERE TelegramID=" + message.Chat.Id;
            int UserTeamNumber = Convert.ToInt32(Sqlcmd.ExecuteScalar());
            connection.Close();
            string[] Teams = { "💿Орёл", "📀Решка" };
            //
            //Считаем потенциальный выигрыш
            double UserWinAmount;
            double UserPercent;
            //Разные действия  зависимости от статуса пользователя в этой игре
            if (UserTeamNumber == 0)
            {
                UserPercent = (UserAmount * 100) / TeamHeadAmount;
                UserWinAmount = UserAmount + TeamTailsAmount * (UserPercent / 100);
            }
            else
            {
                UserPercent = (UserAmount * 100) / TeamTailsAmount;
                UserWinAmount = UserAmount + TeamHeadAmount * (UserPercent / 100);
            }
            //
            await client.SendTextMessageAsync(message.Chat.Id, "Количество участников:"
                 + "\n💿Орёл: " + TeamHeadCout + "  vs  📀Решка: " + TeamTailsCout
                 + "\nКоличество монет по командам:"
                 + "\n💿Орёл: " + TeamHeadAmount + "YTN   vs  📀Решка: " + TeamTailsAmount
                 + "YTN \n💿: " + TeamHeadPercent + "%   vs  📀: " + TeamTailsPercent + "%"
                 + "\n\nВаша команда: " + Teams[UserTeamNumber]
                 + "\nСтавка: " + UserAmount + "YTN"
                 + "\nВаш вклад в команду: " + Math.Round(UserPercent, 2) + "%"
                 + "\nПотенциальный выигрыш: " + UserWinAmount + "YTN");
            await client.SendTextMessageAsync(message.Chat.Id, "Что дальше?", replyMarkup: markup);
        }

        public async void UserDoesNotExistAction(Message message, TelegramBotClient client, SQLiteConnection connection, SQLiteCommand Sqlcmd,
            double TeamHeadAmount, double TeamTailsAmount, int TeamHeadCout, int TeamTailsCout, double TeamHeadPercent, double TeamTailsPercent)
        {
            //Клавиатура с выбором команды
            var markup = new ReplyKeyboardMarkup();
            markup.Keyboard = new KeyboardButton[][]
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
            markup.OneTimeKeyboard = true;

            //
            await client.SendTextMessageAsync(message.Chat.Id, "Количество участников:"
                 + "\n💿Орёл: " + TeamHeadCout + "  vs  📀Решка: " + TeamTailsCout
                 + "\nКоличество монет по командам:"
                 + "\n💿Орёл: " + TeamHeadAmount + "YTN  vs  📀Решка: " + TeamTailsAmount
                 + "YTN\n💿: " + TeamHeadPercent + "%   vs  📀: " + TeamTailsPercent + "%");
            await client.SendTextMessageAsync(message.Chat.Id, "На кого ставим?", replyMarkup: markup);
        }
    }
}
