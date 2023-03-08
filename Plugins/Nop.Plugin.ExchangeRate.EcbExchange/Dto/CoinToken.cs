using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Plugin.ExchangeRate.EcbExchange.Dto
{
    public class CoinToken
    {
        public string symbol { get; set; }
        public decimal price { get; set; }
    }
}
