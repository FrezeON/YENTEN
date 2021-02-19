using System;
using System.Collections.Generic;
using System.Text;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace YENTEN.Command.Commands
{
    public class InformationCommand : Command
    {
        public override string[] Names { get; set; } = new string[] {"info", "information", "Инфа", "Информация" };

        public override async void Execute(Message message, TelegramBotClient client)
        {
           
            await client.SendTextMessageAsync(message.Chat.Id, $"Это бот для игры 'Орел и решка' в качестве ставки используется YENTEN. \nПравила максимально просты:" +
                "\n1.Вы выбираете орел или решку" +"\n2.Выбираете количество монет которые готовы поставить."
+ "\n3.Через 5 мин если в каждой команде есть хотя бы 1 участник прием монет останавливается и выбирается орел или решка(random.org)"
+ "\n4.Команда победитель делит между собой монеты проигравшей команды.С учетом кто сколько внес."
+ "\n\nДля примера:"
+ "\n\nВы внесли 25 монет на команду 'Решка'."
+ "\nВ целом на команду 'Решка' поставили 153 монеты, а на команду 'Орел' поставили 62 монеты."
+ "\nВ случае победы вашей команды вы получите = ваша ставка + 62 * (((25 * 100) / 153) / 100) ~= 35.13"
+ "\nВесь процесс игры и пополнение работает без участия оператора.\nОднако для вывода выигранных средств необходимо подать заявку через специальную форму.");
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
        }
    }
}
