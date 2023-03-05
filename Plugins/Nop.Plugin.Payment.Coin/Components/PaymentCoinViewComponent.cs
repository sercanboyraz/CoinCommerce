using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Stores;
using Nop.Core.Http.Extensions;
using Nop.Plugin.Payments.Coin.Models;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Logging;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Web.Framework.Components;

namespace Nop.Plugin.Payments.Coin.Components
{
    [ViewComponent(Name = "PaymentCoin")]
    public class PaymentCoinViewComponent : NopViewComponent
    {
        private readonly ISettingService _settingService;
        private readonly IStoreContext _storeContext;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly IWorkContext _workContext;
        private readonly IOrderTotalCalculationService _orderTotalCalculationService;
        private readonly ICustomerService _customerService;
        private readonly OrderSettings _orderSettings;
        private readonly IOrderProcessingService _orderProcessingService;
        private readonly IPaymentService _paymentService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly ILogger _logger;
        private readonly ICurrencyService _currencyService;

        public PaymentCoinViewComponent(ISettingService settingService,
            IStoreContext storeContext,
            IShoppingCartService shoppingCartService,
            IWorkContext workContext,
            IOrderTotalCalculationService orderTotalCalculationService,
            ICustomerService customerService,
           OrderSettings orderSettings,
           IOrderProcessingService orderProcessingService,
           IPaymentService paymentService,
           IGenericAttributeService genericAttributeService,
           ILogger logger,
           ICurrencyService currencyService)
        {
            _settingService = settingService;
            _storeContext = storeContext;
            _shoppingCartService = shoppingCartService;
            _workContext = workContext;
            _orderTotalCalculationService = orderTotalCalculationService;
            _customerService = customerService;
            _orderSettings = orderSettings;
            _orderProcessingService = orderProcessingService;
            _paymentService = paymentService;
            _genericAttributeService = genericAttributeService;
            _logger = logger;
            _currencyService = currencyService;
        }

        public IViewComponentResult Invoke()
        {
            var storeScope = _storeContext.GetActiveStoreScopeConfigurationAsync().Result;
            var manualPaymentSettings = _settingService.LoadSettingAsync<CoinPaymentSettings>(storeScope).Result;
            var model = new PaymentInfoModel(manualPaymentSettings.CoinChainInfo)
            {
                CoinChainInfo = manualPaymentSettings.CoinChainInfo,
                WalletAddress = manualPaymentSettings.WalletAddress,
                GasPrice = manualPaymentSettings.GasPrice,
                GasLimit = manualPaymentSettings.GasLimit,
            };

            var customer = _workContext.GetCurrentCustomerAsync().Result;
            var store = _storeContext.GetCurrentStoreAsync().Result;
            var cart = _shoppingCartService.GetShoppingCartAsync(customer, ShoppingCartType.ShoppingCart, store.Id).Result;

            if (!cart.Any())
                return View("~/");

            //if (_orderSettings.OnePageCheckoutEnabled)
            //    return View("~/");

            if (_customerService.IsGuestAsync(customer).Result && !_orderSettings.AnonymousCheckoutAllowed)
                return View("~/");

            decimal? cartTotalPice = _orderTotalCalculationService.GetShoppingCartTotalAsync(cart, manualPaymentSettings.AdditionalFeePercentage).Result.shoppingCartTotal;
            model.OrderTotal = cartTotalPice;

            foreach (var coinChainInfo in model.ParseCoinChainInfo)
            {
                var getCurrencyCode = _currencyService.GetCurrencyByCodeAsync(coinChainInfo.Symbol);
                if (getCurrencyCode != null)
                    coinChainInfo.Price = _currencyService.ConvertCurrency(model.OrderTotal.Value, getCurrencyCode.Result.Rate);
            }

            try
            {
                var processPaymentRequest = new ProcessPaymentRequest();
                //if (processPaymentRequest == null)
                //{
                //    if (_orderProcessingService.IsPaymentWorkflowRequiredAsync(cart).Result)
                //        return View("~/");
                //    processPaymentRequest = new ProcessPaymentRequest();
                //}
                _paymentService.GenerateOrderGuid(processPaymentRequest);
                processPaymentRequest.StoreId = store.Id;
                processPaymentRequest.CustomerId = customer.Id;
                processPaymentRequest.PaymentMethodSystemName = _genericAttributeService.GetAttributeAsync<string>(
                    customer,
                    NopCustomerDefaults.SelectedPaymentMethodAttribute,
                    store.Id).Result;
                HttpContext.Session.Set<ProcessPaymentRequest>("OrderPaymentInfo", processPaymentRequest);
                HttpContext.Session.Set<IList<ShoppingCartItem>>("PaymentCoinViewComponentCart-" + processPaymentRequest.OrderGuid, cart);
                HttpContext.Session.Set<PaymentInfoModel>("PaymentCoinViewComponentModel-" + processPaymentRequest.OrderGuid, model);
                //var placeOrderResult = await _orderProcessingService.PlaceOrderAsync(processPaymentRequest);
                //if (placeOrderResult.Success)
                //{
                //    HttpContext.Session.Set<ProcessPaymentRequest>("OrderPaymentInfo", null);
                //    var postProcessPaymentRequest = new PostProcessPaymentRequest
                //    {
                //        Order = placeOrderResult.PlacedOrder
                //    };
                //    await _paymentService.PostProcessPaymentAsync(postProcessPaymentRequest);
                //}

                //foreach (var error in placeOrderResult.Errors)
                //    model.Warnings.Add(error);
            }
            catch (Exception exc)
            {
                _logger.WarningAsync(exc.Message, exc);
                model.Warnings.Add(exc.Message);
            }




            return View("~/Plugins/Payments.Coin/Views/PaymentInfo.cshtml", model);
        }
    }
}
