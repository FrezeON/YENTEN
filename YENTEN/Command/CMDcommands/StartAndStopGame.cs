using System;
using System.Collections.Generic;
using System.Text;
using System.Timers;
using Telegram.Bot;
using Telegram.Bot.Types;
using YENTEN.Command.Commands.Game;

namespace YENTEN.Command.CMDcommands
{
    class StartAndStopGame : CMDcommand
    {
        private static Timer aTimer;

        public override string[] Names { get; set; } = new string[] { "StartGame", "StopGame" };

        public override void Execute(string comandText)
        {
            if(comandText == "StartGame")
            {
                SetTimer();
            }
            else
            {
                aTimer.Stop();
                aTimer.Dispose();
            }
        }
        public static void SetTimer()
        {
            aTimer = new System.Timers.Timer(20000);

            aTimer.Elapsed += GameProcess.Run;
            aTimer.AutoReset = true;
            aTimer.Enabled = true;
        }
    }
}
