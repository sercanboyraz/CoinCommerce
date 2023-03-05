using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Plugin.Payments.Coin.Models
{
    public class CoinInfoDto
    {
        public int ChainId { get; set; }
        public string NetworkName { get; set; }
        public string Symbol { get; set; }
        public string RpcUrl { get; set; }
        public string ImageUrl { get; set; }
        public decimal Price { get; set; }
    }
}
