using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Text;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace YENTEN.Command.Commands
{
    public class Start : Command
    {
        public override string[] Names { get; set; } = new string[] { "/start", "star", "Старт"};
        private static SQLiteConnection connection;
        public override async void Execute(Message message, TelegramBotClient client)
        {
            connection = new SQLiteConnection("Data Source=MainDB1.db");
            SQLiteCommand Sqlcmd = connection.CreateCommand();
            connection.Open();
            Sqlcmd.CommandText = "SELECT count(TelegramID) FROM UserInfo WHERE TelegramID=" + message.Chat.Id;
            int UserStatus = Convert.ToInt32(Sqlcmd.ExecuteScalar());
            connection.Close();
            if (UserStatus == 0)
            {
                await client.SendTextMessageAsync(message.Chat.Id, $"Это бот для игры 'Орел и решка' в качестве ставки используется YENTEN. \nПравила максимально просты:" +
    "\n1.Вы выбираете орел или решку" + "\n2.Выбираете количество монет которые готовы поставить."
+ "\n3.Через 5 мин если в каждой команде есть хотя бы 1 участник прием монет останавливается и выбирается орел или решка(random.org)"
+ "\n4.Команда победитель делит между собой монеты проигравшей команды.С учетом кто сколько внес."
+ "\n\nДля примера:"
+ "\n\nВы внесли 25 монет на команду 'Решка'."
+ "\nВ целом на команду 'Решка' поставили 153 монеты, а на команду 'Орел' поставили 62 монеты."
+ "\nВ случае победы вашей команды вы получите = ваша ставка + 62 * (((25 * 100) / 153) / 100) ~= 35.13"
+ "\nВесь процесс игры и пополнение работает без участия оператора.\nОднако для вывода выигранных средств необходимо подать заявку через специальную форму.");
                var markup = new ReplyKeyboardMarkup();
                markup.Keyboard = new KeyboardButton[][]
                {
                new KeyboardButton[]
                {
                new KeyboardButton("🔑Регистрация")
                }
                };
                markup.OneTimeKeyboard = true;
                await client.SendTextMessageAsync(message.Chat.Id, "Для начала нажмите кнопку 🔑Регистрация", replyMarkup: markup);
            }
            else
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
                await client.SendTextMessageAsync(message.Chat.Id, "🔍Вы уже зарегестрированы, вот Ваше меню", replyMarkup: markup);
                //
            }

        }
    }
}
