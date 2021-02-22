using System;
using System.Collections.Generic;
using System.Text;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace YENTEN.Inline
{
    class KeyBoards
    {
        public static async void SendPtofileKeyBoardAsync(TelegramBotClient client, Message message)
        {
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
            await client.SendTextMessageAsync(message.Chat.Id, "Куда дальше?", replyMarkup: markup);
            //
        }
    }
}
