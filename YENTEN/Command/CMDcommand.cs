using System;
using System.Collections.Generic;
using System.Text;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace YENTEN.Command
{
   public abstract class CMDcommand
    {
        public abstract string[] Names { get; set; }
        public abstract void Execute(string comandText);
        public bool Contains(string comandText)
        {
            foreach (var comm in Names)
            {
                if (comandText==comm)
                {
                    return true;
                }
            }
            return false;
            
        }
    }
}
