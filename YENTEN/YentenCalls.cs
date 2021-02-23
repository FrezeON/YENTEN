using BitcoinLib.Services.Coins.Cryptocoin;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace YENTEN
{
    class YentenCalls
    {
        internal static decimal GetAdressBalance(string WalletIN)
        {
            ICryptocoinService cryptocoinService = new CryptocoinService("http://localhost:9982", Config.RPClogin, Config.RPCpass, "null", 10);
            return cryptocoinService.GetAddressBalance(WalletIN); 

        }
        internal static decimal GetAdressInputTransaction(string WalletIN)
        {
            ICryptocoinService cryptocoinService = new CryptocoinService("http://localhost:9982", Config.RPClogin, Config.RPCpass, "null", 10);
            decimal returnet = cryptocoinService.GetReceivedByAddress(WalletIN);
            return returnet;

        }
        internal static string GetNewAddress()
        {
            ICryptocoinService cryptocoinService = new CryptocoinService("http://localhost:9982", Config.RPClogin, Config.RPCpass, "null", 10);
            string returnet = cryptocoinService.GetNewAddress();
            return returnet;

        }
        internal static bool SendToAddress(string UserWalet, decimal Amont)
        {
            try
            {
                ICryptocoinService cryptocoinService = new CryptocoinService("http://localhost:9982", Config.RPClogin, Config.RPCpass, "null", 10);
                string returnet = cryptocoinService.SendToAddress(UserWalet, Amont);
                return true;
            }
            catch(Exception e)
            {
                Console.WriteLine(DateTime.Now + "  [Log]: " + e);
                System.IO.File.AppendAllText("log.txt", DateTime.Now + "  [Log]: " + e);
                return false;
            }
        }
    }
}
