using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography.Xml;
using System.Security.Policy;
using System.Threading.Tasks;
using System.Xml;
using Nop.Core;
using Nop.Core.Http;
using Nop.Plugin.ExchangeRate.EcbExchange.Dto;
using Nop.Services.Configuration;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Plugins;

namespace Nop.Plugin.ExchangeRate.EcbExchange
{
    public class EcbExchangeRateProvider : BasePlugin, IExchangeRateProvider
    {
        #region Fields

        private readonly EcbExchangeRateSettings _ecbExchangeRateSettings;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILocalizationService _localizationService;
        private readonly ILogger _logger;
        private readonly ISettingService _settingService;

        #endregion

        #region Ctor

        public EcbExchangeRateProvider(EcbExchangeRateSettings ecbExchangeRateSettings,
            IHttpClientFactory httpClientFactory,
            ILocalizationService localizationService,
            ILogger logger,
            ISettingService settingService)
        {
            _ecbExchangeRateSettings = ecbExchangeRateSettings;
            _httpClientFactory = httpClientFactory;
            _localizationService = localizationService;
            _logger = logger;
            _settingService = settingService;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets currency live rates
        /// </summary>
        /// <param name="exchangeRateCurrencyCode">Exchange rate currency code</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the exchange rates
        /// </returns>
        public async Task<IList<Core.Domain.Directory.ExchangeRate>> GetCurrencyLiveRatesAsync(string exchangeRateCurrencyCode)
        {
            if (exchangeRateCurrencyCode == null)
                throw new ArgumentNullException(nameof(exchangeRateCurrencyCode));

            //add euro with rate 1
            var ratesToEuro = new List<Core.Domain.Directory.ExchangeRate>
            {
                new Core.Domain.Directory.ExchangeRate
                {
                    CurrencyCode = "EUR",
                    Rate = 1,
                    UpdatedOn = DateTime.UtcNow
                }
            };
            List<CoinToken> convertData = new List<CoinToken>();
            DateTime dateupdate = DateTime.UtcNow;
            //get exchange rates to euro from European Central Bank
            try
            {
                var httpClient = _httpClientFactory.CreateClient(NopHttpDefaults.DefaultHttpClient);
                var stream = await httpClient.GetStreamAsync(_ecbExchangeRateSettings.EcbLink);

                //load XML document
                var document = new XmlDocument();
                document.Load(stream);

                //add namespaces
                var namespaces = new XmlNamespaceManager(document.NameTable);
                namespaces.AddNamespace("ns", "http://www.ecb.int/vocabulary/2002-08-01/eurofxref");
                namespaces.AddNamespace("gesmes", "http://www.gesmes.org/xml/2002-08-01");

                //get daily rates
                var dailyRates = document.SelectSingleNode("gesmes:Envelope/ns:Cube/ns:Cube", namespaces);
                if (!DateTime.TryParseExact(dailyRates.Attributes["time"].Value, "yyyy-MM-dd", null, DateTimeStyles.None, out var updateDate))
                    updateDate = DateTime.UtcNow;
                dateupdate = updateDate;
                foreach (XmlNode currency in dailyRates.ChildNodes)
                {
                    //get rate
                    if (!decimal.TryParse(currency.Attributes["rate"].Value, NumberStyles.Currency, CultureInfo.InvariantCulture, out var currencyRate))
                        continue;

                    ratesToEuro.Add(new Core.Domain.Directory.ExchangeRate()
                    {
                        CurrencyCode = currency.Attributes["currency"].Value,
                        Rate = currencyRate,
                        UpdatedOn = updateDate
                    });
                }
                try
                {
                    var client = new HttpClient();
                    client.BaseAddress = new Uri("https://api.binance.com/api/v3/ticker/price");
                    // Add an Accept header for JSON format.
                    client.DefaultRequestHeaders.Accept.Add(
                       new MediaTypeWithQualityHeaderValue("application/json"));
                    // Get data response
                    var response = client.GetAsync("").Result;
                    if (response.IsSuccessStatusCode)
                    {
                        // Parse the response body
                        var dataObjects = response.Content.ReadAsStringAsync().Result;
                        convertData = Newtonsoft.Json.JsonConvert.DeserializeObject<List<CoinToken>>(dataObjects);
                        var getBTC = convertData.FirstOrDefault(x => x.symbol == "BTCEUR");
                        ratesToEuro.Add(new Core.Domain.Directory.ExchangeRate()
                        {
                            CurrencyCode = "BTC",
                            Rate = 1 / getBTC.price,
                            UpdatedOn = updateDate
                        });
                        var getETH = convertData.FirstOrDefault(x => x.symbol == "ETHEUR");
                        ratesToEuro.Add(new Core.Domain.Directory.ExchangeRate()
                        {
                            CurrencyCode = "ETH",
                            Rate = 1 / getETH.price,
                            UpdatedOn = updateDate
                        });
                        var getBNB = convertData.FirstOrDefault(x => x.symbol == "BNBEUR");
                        ratesToEuro.Add(new Core.Domain.Directory.ExchangeRate()
                        {
                            CurrencyCode = "BNB",
                            Rate = 1 / getBNB.price,
                            UpdatedOn = updateDate
                        });


                    }
                    else
                    {
                        await _logger.ErrorAsync("Coin-1 exchange rate provider.StatusCode" + (int)response.StatusCode + ". ReasonPhrase" + response.Content);
                    }
                }
                catch (Exception ex)
                {
                    await _logger.ErrorAsync("Coin exchange rate provider", ex);
                }
            }
            catch (Exception ex)
            {
                await _logger.ErrorAsync("ECB exchange rate provider", ex);
            }

            //return result for the euro
            if (exchangeRateCurrencyCode.Equals("eur", StringComparison.InvariantCultureIgnoreCase))
                return ratesToEuro;

            //use only currencies that are supported by ECB
            var exchangeRateCurrency = ratesToEuro.FirstOrDefault(rate => rate.CurrencyCode.Equals(exchangeRateCurrencyCode, StringComparison.InvariantCultureIgnoreCase));
            if (exchangeRateCurrency == null)
                throw new NopException(await _localizationService.GetResourceAsync("Plugins.ExchangeRate.EcbExchange.Error"));

            var getUSD = ratesToEuro.FirstOrDefault(rate => rate.CurrencyCode.Equals("usd", StringComparison.InvariantCultureIgnoreCase));
            ratesToEuro.Add(new Core.Domain.Directory.ExchangeRate()
            {
                CurrencyCode = "USDT",
                Rate = getUSD.Rate,
                UpdatedOn = dateupdate
            });

            ratesToEuro.Add(new Core.Domain.Directory.ExchangeRate()
            {
                CurrencyCode = "USDC",
                Rate = getUSD.Rate,
                UpdatedOn = dateupdate
            });

            //return result for the selected (not euro) currency
            return ratesToEuro.Select(rate => new Core.Domain.Directory.ExchangeRate
            {
                CurrencyCode = rate.CurrencyCode,
                Rate = Math.Round(rate.Rate / exchangeRateCurrency.Rate, 8),
                UpdatedOn = rate.UpdatedOn
            }).ToList();
        }

        /// <summary>
        /// Install the plugin
        /// </summary>
        /// <returns>A task that represents the asynchronous operation</returns>
        public override async Task InstallAsync()
        {
            //settings
            var defaultSettings = new EcbExchangeRateSettings
            {
                EcbLink = "https://www.ecb.europa.eu/stats/eurofxref/eurofxref-daily.xml"
            };
            await _settingService.SaveSettingAsync(defaultSettings);

            //locales
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.ExchangeRate.EcbExchange.Error", "You can use ECB (European central bank) exchange rate provider only when the primary exchange rate currency is supported by ECB");

            await base.InstallAsync();
        }

        /// <summary>
        /// Uninstall the plugin
        /// </summary>
        /// <returns>A task that represents the asynchronous operation</returns>
        public override async Task UninstallAsync()
        {
            //settings
            await _settingService.DeleteSettingAsync<EcbExchangeRateSettings>();

            //locales
            await _localizationService.DeleteLocaleResourceAsync("Plugins.ExchangeRate.EcbExchange.Error");

            await base.UninstallAsync();
        }

        #endregion

    }
}