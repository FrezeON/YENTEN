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
        public string ACCOUNT_NAME = "YentenLucky";
        internal static decimal GetAdressBalance(string WalletIN)
        {
            ICryptocoinService cryptocoinService = new CryptocoinService("http://localhost:9982", "user", "x", "null", 5);
            return cryptocoinService.GetAddressBalance(WalletIN); 

        }
        internal static decimal GetAdressInputTransaction(string WalletIN)
        {
            ICryptocoinService cryptocoinService = new CryptocoinService("http://localhost:9982", "user", "x", "null", 5);
            decimal returnet = cryptocoinService.GetReceivedByAddress(WalletIN);
            return returnet;

        }
    }
}
