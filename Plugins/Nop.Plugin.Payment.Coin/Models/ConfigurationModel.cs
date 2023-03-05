using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Mvc.ModelBinding;
using Nop.Web.Framework.Models;

namespace Nop.Plugin.Payments.Coin.Models
{
    public record ConfigurationModel : BaseNopModel
    {
        public int ActiveStoreScopeConfiguration { get; set; }

        public int TransactModeId { get; set; }
        [NopResourceDisplayName("Plugins.Payments.Coin.Fields.TransactMode")]
        public SelectList TransactModeValues { get; set; }
        public bool TransactModeId_OverrideForStore { get; set; }


        [NopResourceDisplayName("Plugins.Payments.Coin.Fields.GasPrice")]
        public decimal GasPrice { get; set; }
        public bool GasPrice_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Payments.Coin.Fields.GasLimit")]
        public decimal GasLimit { get; set; }
        public bool GasLimit_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Payments.Coin.Fields.WalletAddress")]
        public string WalletAddress { get; set; }
        public bool WalletAddress_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Payments.Coin.Fields.CoinChainInfo")]
        public string CoinChainInfo { get; set; }
        public bool CoinChainInfo_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Payments.Manual.Fields.AdditionalFeePercentage")]
        public bool AdditionalFeePercentage { get; set; }
        public bool AdditionalFeePercentage_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Payments.Manual.Fields.AdditionalFee")]
        public decimal AdditionalFee { get; set; }
        public bool AdditionalFee_OverrideForStore { get; set; }
    }
}