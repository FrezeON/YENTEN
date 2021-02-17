using System;
using System.Collections.Generic;
using System.Text;
using Telegram.Bot;
using Telegram;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot.Types;

namespace YENTEN.Inline
{
    class MainMenu
    {
        public static async void SendMAinMenu(TelegramBotClient client, Message message)
        {
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
            await client.SendTextMessageAsync(message.Chat.Id, "Вот ваше меню", replyMarkup: markup);

        }
    }
}
