using System;
using System.Collections.Generic;
using System.Text;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace YENTEN.Command.Commands
{
    public class Start : Command
    {
        public override string[] Names { get; set; } = new string[] { "/start", "star", "Старт"};

        public override async void Execute(Message message, TelegramBotClient client)
        {
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
    }
}
