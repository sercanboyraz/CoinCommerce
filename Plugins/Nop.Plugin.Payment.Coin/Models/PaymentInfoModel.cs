using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Mvc.ModelBinding;
using Nop.Web.Framework.Models;
using LinqToDB.Common;

namespace Nop.Plugin.Payments.Coin.Models
{
    public record PaymentInfoModel : BaseNopModel
    {
        public PaymentInfoModel()
        {
            if (!CoinChainInfo.IsNullOrWhiteSpace())
                ParseCoinChainInfo = Newtonsoft.Json.JsonConvert.DeserializeObject<List<CoinInfoDto>>(CoinChainInfo);
        }

        public PaymentInfoModel(string coinChainInfo)
        {
            if (!coinChainInfo.IsNullOrWhiteSpace())
                ParseCoinChainInfo = Newtonsoft.Json.JsonConvert.DeserializeObject<List<CoinInfoDto>>(coinChainInfo);
        }

        [NopResourceDisplayName("Plugins.Payments.Coin.Fields.GasPrice")]
        public decimal GasPrice { get; set; }

        [NopResourceDisplayName("Plugins.Payments.Coin.Fields.GasLimit")]
        public decimal GasLimit { get; set; }

        [NopResourceDisplayName("Plugins.Payments.Coin.Fields.WalletAddress")]
        public string WalletAddress { get; set; }

        [NopResourceDisplayName("Plugins.Payments.Coin.Fields.CoinChainInfo")]
        public string CoinChainInfo { get; set; }
        public List<CoinInfoDto> ParseCoinChainInfo { get; set; }

        public int? SelectChainId { get; set; }

        public decimal? OrderTotal { get; set; }

        public IList<string> Warnings { get; set; }
    }
}