using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SQLite;
using System.IO;
using Telegram.Bot;
using Telegram.Bot.Types;
using System.Text.RegularExpressions;

namespace YENTEN.Command.CMDcommands
{
    public class ImputNewWallet : CMDcommand
    {
        private SQLiteConnection connection;

        public override string[] Names { get; set; } = new string[] { "inputWallet", "addWallet" };

        public override async  void Execute(string comandText)
        {
            
            Console.WriteLine("Запуск команды InputNewWallet");
            //копируем кошельки из файла

            string RawWallets = await System.IO.File.ReadAllTextAsync("newWallets.txt");
            Match match = Regex.Match(RawWallets, "\"\",\"(.*?)\"");
            int Count = 0;
            while(match.Groups[1].Value != "")
            {
                Count++;
                string queryString = "INSERT INTO RawWallets VALUES('"+ match.Groups[1].Value+"');";
                DatabaseLibrary.ExecuteNonQuery(queryString);
                match= match.NextMatch();
            }
            Console.WriteLine("Add  " + (Count)+"  Wallets");              

            
        }
    }
}
