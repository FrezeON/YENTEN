using System;
using System.Collections.Generic;
using Telegram.Bot;
using Telegram.Bot.Args;
using YENTEN.Command.Commands;
using System.Data.SQLite;
using YENTEN.Command.CMDcommands;
using Telegram.Bot.Types.Enums;
using YENTEN.Inline;
using YENTEN.Command.Commands.Game;
using System.Timers;

namespace YENTEN
{
    class Program
    {
        private static Timer aTimer;
        private static TelegramBotClient client;
        private static List<Command.Command> commands;
        private static List<Command.CMDcommand> commandsCMD;
        private static SQLiteConnection connection;

        static void Main(string[] args)
        {
            client = new TelegramBotClient(Config.Token);
            //Cписок команд начинается здесь
            commands = new List<Command.Command>();
            commands.Add(new InformationCommand());
            commands.Add(new RegistrationCheck());
            commands.Add(new CallMainMenu());
            commands.Add(new Start());
            commands.Add(new Profile());
            commands.Add(new BallanceCheck());
            commands.Add(new EnterTheGame());
            commands.Add(new GameRegistration());
            commands.Add(new GameHistory());
            commands.Add(new WithdrawFunds());
            //Cписок команд заканчивается здесь

            //Cписок команд начинается здесь (для консоли)
            commandsCMD = new List<Command.CMDcommand>();
            commandsCMD.Add(new ImputNewWallet());
            commandsCMD.Add(new StartAndStopGame());
            commandsCMD.Add(new SendNotif());
            // Cписок команд заканчивается здесь(для консоли)
            SetTimer();
            client.StartReceiving();
            client.OnMessage += OnMessageHandler;
            Console.WriteLine(DateTime.Now+ "  [Log]: Bot started");
            for (; ;)
            {
                    OnConsoleHandler();
            }
            

        }
        private static void OnConsoleHandler()
        {
            string commandText = Console.ReadLine();
            if (commandText != null)
            {
                foreach(var command in commandsCMD)
                {
                    if (command.Contains(commandText))
                    {
                        command.Execute(commandText);
                    }
                }
            }
        }
        private static void OnMessageHandler(object sender, MessageEventArgs e)
        {
            var message = e.Message;
            
            if (message.Text != null)
            {
                foreach(var comm in commands)
                {
                    if (comm.Contains(message.Text))
                    {
                        comm.Execute(message, client);
                    }
                  
                }
            }
           if (message.ReplyToMessage != null && message.ReplyToMessage.Text == "Вставсте адресс вашего колька." +
                    "\nAдрес кошелька должен быть похож на это: YnNhhuHjnqpk86fdiXd3SXo5DXozEE4Wxv"
                    + "\nВ случае неправильного ввода авдресса необходимо обратится к оператору @UtkaZapas")
           {
                Registration.StrartReg(message, client);
           }
           if (message.ReplyToMessage != null && message.ReplyToMessage.Text =="Введите Вашу ставку в формате 14 или (14.531)")
            {
                GameRegistration.UserReg(message, client, connection);
            }
           if (message.ReplyToMessage != null && message.ReplyToMessage.Text == "Подтверждаю")
            {
                WithdrawFunds.WithdrawFundsApprowed(message, client);
            }
        }

        public static void SetTimer()
        {
            aTimer = new System.Timers.Timer(10000);

            aTimer.Elapsed += NotifyUserAfterGame.SenNotification;
            aTimer.AutoReset = true;
            aTimer.Enabled = true;
        }
    }
}
