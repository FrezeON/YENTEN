using System;
using System.Collections.Generic;
using System.Text;
using Telegram.Bot;
using Telegram.Bot.Types;
using YENTEN.Inline;

namespace YENTEN.Command.Commands
{
    public class CallMainMenu : Command
    {
        public override string[] Names { get; set; } = new string[] {"Меню", "Menu"};

        public override async void Execute(Message message, TelegramBotClient client)
        {
            MainMenu.SendMAinMenu(client, message);
        }
    }
}
