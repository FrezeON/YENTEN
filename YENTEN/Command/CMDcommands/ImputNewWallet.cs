using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SQLite;
using System.IO;

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

            string RawWallets = await File.ReadAllTextAsync(@"D:\YentLuckyBot\newWallets.txt");
            string[] Wallets = RawWallets.Split(new char[] { ' ' });
            //

            //Вносим кошельки в конец базы
            connection = new SQLiteConnection("Data Source=MainDB1.db");
            connection.Open();
            SQLiteCommand Sqlcmd = connection.CreateCommand();
                  for (int i=0; i< Wallets.Length; i++)
                  {
                    Sqlcmd.CommandText = "INSERT INTO RawWallets VALUES(@Wallet)";
                    Sqlcmd.Parameters.AddWithValue("@Wallet", Wallets[i]);
                    Sqlcmd.ExecuteNonQuery();
                  }

            Console.WriteLine("Add  " + (Wallets.Length)+"  Wallets");              
            connection.Close();

            
        }
    }
}
