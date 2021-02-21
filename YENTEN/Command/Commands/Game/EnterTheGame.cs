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
        public override string[] Names { get; set; } = new string[] { "Игра", "Войти в игру" };

        public override void Execute(Message message, TelegramBotClient client)
        {


            //Подсчет суммы по командам     Количество игроков в командах     Head - орёл Tails- Решка
            decimal TeamHeadAmount =0;
            decimal TeamTailsAmount =0;
            int TeamHeadCout = 0;
            int TeamTailsCout = 0;
            string queryString = "SELECT max(rowid) FROM CurrentGame";
            int maxRowID = DatabaseLibrary.ExecuteScalarInt(queryString);
            queryString = "SELECT min(rowid) FROM CurrentGame";
            int minRowID = DatabaseLibrary.ExecuteScalarInt(queryString);
            for (int i = minRowID; i < maxRowID+1; i++)
            {
                queryString = "SELECT AmountYTN FROM CurrentGame WHERE rowid="+i;
                decimal Amount = DatabaseLibrary.ExecuteScalarDecimal(queryString);
                queryString = "SELECT Team FROM CurrentGame WHERE rowid=" + i;
                int TeamNumber = DatabaseLibrary.ExecuteScalarInt(queryString) ;
                //Console.WriteLine(TeamNumber + "      " + Amount);
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
            //

            //Процентное соотношение по балансу
            decimal TeamHeadPercent = 0;
            decimal TeamTailsPercent = 0;
            if (TeamHeadAmount != 0)
            {
                 TeamHeadPercent = Math.Round((TeamHeadAmount * 100) / (TeamHeadAmount + TeamTailsAmount), 2);
                 TeamTailsPercent = Math.Round(100 - TeamHeadPercent, 2);
            }

            //       
            //Поиск пользователя в игре
            queryString = "SELECT COUNT(*) FROM CurrentGame WHERE TelegramID=" + message.Chat.Id;
            int UserExist = DatabaseLibrary.ExecuteScalarInt(queryString);
            queryString = "SELECT AmountYTN FROM CurrentGame WHERE TelegramID=" + message.Chat.Id;
            decimal AmountInCurrentGame = DatabaseLibrary.ExecuteScalarDecimal(queryString);
            //
            if(UserExist == 0 || AmountInCurrentGame ==0)
            {
                UserDoesNotExistAction(message, client, TeamHeadAmount, TeamTailsAmount, TeamHeadCout, TeamTailsCout, TeamHeadPercent, TeamTailsPercent);
            }
            else
            {
                UserExistAction(message, client, TeamHeadCout, TeamTailsCout, TeamHeadAmount, TeamTailsAmount, TeamHeadPercent, TeamTailsPercent );
            }
            
        }
        public async void UserExistAction(Message message, TelegramBotClient client, int TeamHeadCout, int TeamTailsCout, decimal TeamHeadAmount,
            decimal TeamTailsAmount, decimal TeamHeadPercent, decimal TeamTailsPercent)
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
                    new KeyboardButton("👤Профиль")
                }
            };
            markup.OneTimeKeyboard = true;
            await client.SendTextMessageAsync(message.Chat.Id, "Куда дальше?", replyMarkup: markup);
            //
            //Берем данные пользователя
            string queryString = "SELECT AmountYTN FROM CurrentGame WHERE TelegramID=" + message.Chat.Id;
            decimal UserAmount = DatabaseLibrary.ExecuteScalarDecimal(queryString);
            queryString = "SELECT Team FROM CurrentGame WHERE TelegramID=" + message.Chat.Id;
            int UserTeamNumber = DatabaseLibrary.ExecuteScalarInt(queryString);
            string[] Teams = { "💿Орёл", "📀Решка" };
            //
            //Считаем потенциальный выигрыш
            decimal UserWinAmount;
            decimal UserPercent;
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
            if (TeamHeadPercent == 0)
            {
                await client.SendTextMessageAsync(message.Chat.Id, "Количество участников:"
                + "\n💿Орёл: " + 0 + "  vs  📀Решка: " + TeamTailsCout
                + "\nКоличество монет по командам:"
                + "\n💿Орёл: " + TeamHeadAmount + "YTN   vs  📀Решка: " + TeamTailsAmount
                + "YTN \n💿: " + TeamHeadPercent + "%   vs  📀: " + TeamTailsPercent + "%"
                + "\n\nВаша команда: " + Teams[UserTeamNumber]
                + "\nСтавка: " + UserAmount + "YTN"
                + "\nВаш вклад в команду: " + Math.Round(UserPercent, 2) + "%"
                + "\nПотенциальный выигрыш: " + UserWinAmount + "YTN"
                + "\n👥Недостаточно игроков для начала игры!");
            }
            else if (TeamHeadCout >= 1 && TeamTailsCout >= 1)
            {
                queryString = "SELECT GameTime FROM NextGameTime WHERE GameTime !=0";
                string StartTime = DatabaseLibrary.ExecuteScalarString(queryString);

                await client.SendTextMessageAsync(message.Chat.Id, "Количество участников:"
                     + "\n💿Орёл: " + (TeamHeadCout) + "  vs  📀Решка: " + TeamTailsCout
                     + "\nКоличество монет по командам:"
                     + "\n💿Орёл: " + TeamHeadAmount + "YTN   vs  📀Решка: " + TeamTailsAmount
                     + "YTN \n💿: " + TeamHeadPercent + "%   vs  📀: " + TeamTailsPercent + "%"
                     + "\n\nВаша команда: " + Teams[UserTeamNumber]
                     + "\nСтавка: " + UserAmount + "YTN"
                     + "\nВаш вклад в команду: " + Math.Round(UserPercent, 2) + "%"
                     + "\nПотенциальный выигрыш: " + UserWinAmount + "YTN"
                     + "\n⏰Раунд закончится: " + StartTime +" МСК");
            }
            else
            {
                await client.SendTextMessageAsync(message.Chat.Id, "Количество участников:"
              + "\n💿Орёл: " + TeamHeadCout + "  vs  📀Решка: " + TeamTailsCout
              + "\nКоличество монет по командам:"
              + "\n💿Орёл: " + TeamHeadAmount + "YTN   vs  📀Решка: " + TeamTailsAmount
              + "YTN \n💿: " + TeamHeadPercent + "%   vs  📀: " + TeamTailsPercent + "%"
              + "\n\nВаша команда: " + Teams[UserTeamNumber]
              + "\nСтавка: " + UserAmount + "YTN"
              + "\nВаш вклад в команду: " + Math.Round(UserPercent, 2) + "%"
              + "\nПотенциальный выигрыш: " + UserWinAmount + "YTN"
              + "\n👥Недостаточно игроков для начала игры!");
            }
            await client.SendTextMessageAsync(message.Chat.Id, "Что дальше?", replyMarkup: markup);
        }

        public async void UserDoesNotExistAction(Message message, TelegramBotClient client,
            decimal TeamHeadAmount, decimal TeamTailsAmount, int TeamHeadCout, int TeamTailsCout, decimal TeamHeadPercent, decimal TeamTailsPercent)
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
            if (TeamHeadPercent == 0)
            {
                await client.SendTextMessageAsync(message.Chat.Id, "Количество участников:"
                + "\n💿Орёл: " + 0 + "  vs  📀Решка: " + TeamTailsCout
                + "\nКоличество монет по командам:"
                + "\n💿Орёл: " + TeamHeadAmount + "YTN  vs  📀Решка: " + TeamTailsAmount
                + "YTN\n💿: " + TeamHeadPercent + "%   vs  📀: " + TeamTailsPercent + "%" 
                + "\n👥Недостаточно игроков для начала игры!");
            }
            else if (TeamHeadCout>=1&& TeamTailsCout >=1)
            {
                string queryString = "SELECT GameTime FROM NextGameTime WHERE GameTime !=0";
                string StartTime = DatabaseLibrary.ExecuteScalarString(queryString);

                await client.SendTextMessageAsync(message.Chat.Id, "Количество участников:"
                + "\n💿Орёл: " + TeamHeadCout + "  vs  📀Решка: " + TeamTailsCout
                + "\nКоличество монет по командам:"
                + "\n💿Орёл: " + TeamHeadAmount + "YTN  vs  📀Решка: " + TeamTailsAmount
                + "YTN\n💿: " + TeamHeadPercent + "%   vs  📀: " + TeamTailsPercent + "%"
                + "\n⏰Раунд закончится: " + StartTime+" МСК");
            }
            else
            {
                await client.SendTextMessageAsync(message.Chat.Id, "Количество участников:"
                + "\n💿Орёл: " + TeamHeadCout + "  vs  📀Решка: " + TeamTailsCout
                + "\nКоличество монет по командам:"
                + "\n💿Орёл: " + TeamHeadAmount + "YTN  vs  📀Решка: " + TeamTailsAmount
                + "YTN\n💿: " + TeamHeadPercent + "%   vs  📀: " + TeamTailsPercent + "%"
                + "\n👥Недостаточно игроков для начала игры!");
            }


            //
            await client.SendTextMessageAsync(message.Chat.Id, "На кого ставим?", replyMarkup: markup);
        }
    }
}
