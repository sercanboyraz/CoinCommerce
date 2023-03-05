using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Nop.Web.Framework.Mvc.Routing;

namespace Nop.Plugin.Payments.Coin.Infrastructure
{
    /// <summary>
    /// Represents plugin route provider
    /// </summary>
    public class RouteProvider : IRouteProvider
    {
        /// <summary>
        /// Register routes
        /// </summary>
        /// <param name="endpointRouteBuilder">Route builder</param>
        public void RegisterRoutes(IEndpointRouteBuilder endpointRouteBuilder)
        {
            endpointRouteBuilder.MapControllerRoute("Plugin.Payments.Coin.WalletCoin", "Plugins/PaymentCoin/WalletCoin",
                 new { controller = "WalletCoin", action = "GetSessionInfo" });

            endpointRouteBuilder.MapControllerRoute("Plugin.Payments.Coin.Confirm", "Plugins/PaymentCoin/Confirm",
                 new { controller = "WalletCoin", action = "Confirm" });

            endpointRouteBuilder.MapControllerRoute("Plugin.Payments.Coin.Error", "Plugins/PaymentCoin/Error",
                 new { controller = "WalletCoin", action = "Error" });
        }

        /// <summary>
        /// Gets a priority of route provider
        /// </summary>
        public int Priority => 0;
    }
}