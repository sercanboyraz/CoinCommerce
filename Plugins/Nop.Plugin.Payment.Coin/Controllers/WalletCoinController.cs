using System;
using System.Threading.Tasks;
using DocumentFormat.OpenXml.EMMA;
using LinqToDB.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Core.Http.Extensions;
using Nop.Plugin.Payments.Coin.Models;
using Nop.Services;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Security;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;

namespace Nop.Plugin.Payments.Coin.Controllers
{
    [AutoValidateAntiforgeryToken]
    public class WalletCoinController : BasePaymentController
    {
        #region Fields
        private readonly ICurrencyService _currencyService;
        private readonly IOrderProcessingService _orderProcessingService;
        private readonly IPaymentService _paymentService;
        private readonly IWebHelper _webHelper;
        private readonly ILocalizationService _localizationService;
        private readonly ILogger _logger;
        private readonly IWorkContext _workContext;
        private readonly ICustomerService _customerService;


        #endregion

        #region Ctor

        public WalletCoinController(ICurrencyService currencyService,
            IOrderProcessingService orderProcessingService,
            IPaymentService paymentService,
            IWebHelper webHelper,
            ILocalizationService localizationService,
            ILogger logger,
            IWorkContext workContext,
            ICustomerService customerService)
        {
            _currencyService = currencyService;
            _orderProcessingService = orderProcessingService;
            _paymentService = paymentService;
            _webHelper = webHelper;
            _localizationService = localizationService;
            _logger = logger;
            _workContext = workContext;
            _customerService = customerService;
        }

        #endregion

        #region Methods

        [HttpGet]
        public async Task<IActionResult> GetSessionInfo(string currencyCode)
        {
            var processPaymentRequest = HttpContext.Session.Get<ProcessPaymentRequest>("OrderPaymentInfo");
            var model = HttpContext.Session.Get<PaymentInfoModel>("PaymentCoinViewComponentModel-" + processPaymentRequest.OrderGuid);
            var getCurrencyCode = _currencyService.GetCurrencyByCodeAsync(currencyCode);
            if (getCurrencyCode != null)
            {
                model.OrderTotal = _currencyService.ConvertCurrency(model.OrderTotal.Value, getCurrencyCode.Result.Rate);
                model.SelectChainId = model.ParseCoinChainInfo.FirstOrDefault(x => x.Symbol == currencyCode).ChainId;
            }
            else
                throw new NopException("ChainId not found!");
            return Json(model);
        }

        [HttpGet]
        public async Task<IActionResult> Confirm(string txt)
        {
            var processPaymentRequest = HttpContext.Session.Get<ProcessPaymentRequest>("OrderPaymentInfo");
            if (txt.IsNullOrWhiteSpace())
            {
                await _logger.ErrorAsync("txt record is null!!!" + Newtonsoft.Json.JsonConvert.SerializeObject(processPaymentRequest));
                return RedirectToRoute("ShoppingCart");
            }

            processPaymentRequest.CustomValues.Add("coinvalidationcode", txt);
            var placeOrderResult = await _orderProcessingService.PlaceOrderAsync(processPaymentRequest);
            if (placeOrderResult.Success)
            {
                HttpContext.Session.Set<ProcessPaymentRequest>("OrderPaymentInfo", null);
                HttpContext.Session.Set<IList<ShoppingCartItem>>("PaymentCoinViewComponentCart-" + processPaymentRequest.OrderGuid, null);
                HttpContext.Session.Set<PaymentInfoModel>("PaymentCoinViewComponentModel-" + processPaymentRequest.OrderGuid, null);
                var postProcessPaymentRequest = new PostProcessPaymentRequest
                {
                    Order = placeOrderResult.PlacedOrder
                };
                await _paymentService.PostProcessPaymentAsync(postProcessPaymentRequest);

                if (_webHelper.IsRequestBeingRedirected || _webHelper.IsPostBeingDone)
                {
                    //redirection or POST has been done in PostProcessPayment
                    return Content(await _localizationService.GetResourceAsync("Checkout.RedirectMessage"));
                }
                await _workContext.SetCurrentCustomerAsync(await _customerService.GetCustomerByIdAsync(placeOrderResult.PlacedOrder.CustomerId));
                return Json(new { orderId = placeOrderResult.PlacedOrder.Id });
            }
            return RedirectToRoute("ShoppingCart");
        }

        [HttpGet]
        public async Task<IActionResult> Error(string error)
        {
            await _logger.ErrorAsync(error);
            return RedirectToRoute("ShoppingCart");
        }
        #endregion
    }
}