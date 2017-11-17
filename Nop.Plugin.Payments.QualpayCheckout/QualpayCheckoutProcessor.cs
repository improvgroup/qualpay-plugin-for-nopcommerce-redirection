using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Core.Plugins;
using Nop.Plugin.Payments.QualpayCheckout.Controllers;
using Nop.Plugin.Payments.QualpayCheckout.Domain;
using Nop.Plugin.Payments.QualpayCheckout.Helpers;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Orders;
using Nop.Services.Payments;

namespace Nop.Plugin.Payments.QualpayCheckout
{
    /// <summary>
    /// Represents Qualpay Checkout processor
    /// </summary>
    public class QualpayCheckoutProcessor : BasePlugin, IPaymentMethod
    {
        #region Fields

        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ICurrencyService _currencyService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly ILocalizationService _localizationService;
        private readonly ILogger _logger;
        private readonly IOrderTotalCalculationService _orderTotalCalculationService;
        private readonly ISettingService _settingService;
        private readonly IWebHelper _webHelper;
        private readonly QualpayCheckoutSettings _qualpayCheckoutSettings;

        #endregion

        #region Ctor

        public QualpayCheckoutProcessor(IHttpContextAccessor httpContextAccessor,
            ICurrencyService currencyService,
            IGenericAttributeService genericAttributeService,
            ILocalizationService localizationService,
            ILogger logger,
            IOrderTotalCalculationService orderTotalCalculationService,
            ISettingService settingService,
            IWebHelper webHelper,
            QualpayCheckoutSettings qualpayCheckoutSettings)
        {
            this._httpContextAccessor = httpContextAccessor;
            this._currencyService = currencyService;
            this._genericAttributeService = genericAttributeService;
            this._localizationService = localizationService;
            this._logger = logger;
            this._orderTotalCalculationService = orderTotalCalculationService;
            this._settingService = settingService;
            this._webHelper = webHelper;
            this._qualpayCheckoutSettings = qualpayCheckoutSettings;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Process a payment
        /// </summary>
        /// <param name="processPaymentRequest">Payment info required for an order processing</param>
        /// <returns>Process payment result</returns>
        public ProcessPaymentResult ProcessPayment(ProcessPaymentRequest processPaymentRequest)
        {
            return new ProcessPaymentResult();
        }

        /// <summary>
        /// Post process payment (used by payment gateways that require redirecting to a third-party URL)
        /// </summary>
        /// <param name="postProcessPaymentRequest">Payment info required for an order processing</param>
        public void PostProcessPayment(PostProcessPaymentRequest postProcessPaymentRequest)
        {
            //get USD currency
            var usdCurrency = _currencyService.GetCurrencyByCode("USD");
            if (usdCurrency == null)
                throw new NopException("USD currency could not be loaded");

            //get order amount in USD currency
            var amount = _currencyService.ConvertFromPrimaryStoreCurrency(postProcessPaymentRequest.Order.OrderTotal, usdCurrency);

            //store location
            var storeLocation = _webHelper.GetStoreLocation();

            //create checkout request
            var checkoutRequest = new QualpayCheckoutRequest
            {
                Amount = Math.Round(amount, 2),
                //ISO numeric code of USD
                CurrencyIsoCode = 840,
                PurchaseId = postProcessPaymentRequest.Order.CustomOrderNumber,
                CustomerFirstName = postProcessPaymentRequest.Order.BillingAddress?.FirstName,
                CustomerLastName = postProcessPaymentRequest.Order.BillingAddress?.LastName,
                CustomerEmail = postProcessPaymentRequest.Order.BillingAddress?.Email,
                //set billing address, max length is 20
                BllingAddress = CommonHelper.EnsureMaximumLength(postProcessPaymentRequest.Order.BillingAddress?.Address1, 20),
                BllingCity = postProcessPaymentRequest.Order.BillingAddress?.City,
                BllingState = postProcessPaymentRequest.Order.BillingAddress?.StateProvince?.Abbreviation,
                BllingZip = postProcessPaymentRequest.Order.BillingAddress?.ZipPostalCode,
                Preferences = new Preferences
                {
                    AllowPartialPayments = false,
                    EnableEmailReceipts = _qualpayCheckoutSettings.EnableEmailReceipts,
                    //20 minutes
                    ExpirationTime = 1200, 
                    RequestType = RequestType.Sale,
                    SuccessUrl = $"{storeLocation}checkout/completed/{postProcessPaymentRequest.Order.Id}",
                    FailureUrl = $"{storeLocation}orderdetails/{postProcessPaymentRequest.Order.Id}",
                    NotificationUrl = $"{storeLocation}Plugins/QualpayCheckout/IPN"
                }
            };

            //get checkout link
            var checkoutResponse = QualpayCheckoutHelper.PostCheckoutRequest(checkoutRequest, _qualpayCheckoutSettings, _logger);
            if (checkoutResponse != null && checkoutResponse.ResponseCode == ResponseCode.OK && checkoutResponse.Details != null)
            {
                //save some of data for the further validation
                _genericAttributeService.SaveAttribute(postProcessPaymentRequest.Order, "QualpayCheckoutId", checkoutResponse.Details.CheckoutId);

                //redirect to Qualpay Checkout
                _httpContextAccessor.HttpContext.Response.Redirect(checkoutResponse.Details.CheckoutLink);
            }
            else
                _httpContextAccessor.HttpContext.Response.Redirect($"{storeLocation}orderdetails/{postProcessPaymentRequest.Order.Id}");
        }

        /// <summary>
        /// Returns a value indicating whether payment method should be hidden during checkout
        /// </summary>
        /// <param name="cart">Shoping cart</param>
        /// <returns>true - hide; false - display.</returns>
        public bool HidePaymentMethod(IList<ShoppingCartItem> cart)
        {
            //you can put any logic here
            //for example, hide this payment method if all products in the cart are downloadable
            //or hide this payment method if current customer is from certain country
            return false;
        }

        /// <summary>
        /// Gets additional handling fee
        /// </summary>
        /// <param name="cart">Shoping cart</param>
        /// <returns>Additional handling fee</returns>
        public decimal GetAdditionalHandlingFee(IList<ShoppingCartItem> cart)
        {
            var result = this.CalculateAdditionalFee(_orderTotalCalculationService, cart,
                _qualpayCheckoutSettings.AdditionalFee, _qualpayCheckoutSettings.AdditionalFeePercentage);

            return result;
        }

        /// <summary>
        /// Captures payment
        /// </summary>
        /// <param name="capturePaymentRequest">Capture payment request</param>
        /// <returns>Capture payment result</returns>
        public CapturePaymentResult Capture(CapturePaymentRequest capturePaymentRequest)
        {
            return new CapturePaymentResult { Errors = new[] { "Capture method not supported" } };
        }

        /// <summary>
        /// Refunds a payment
        /// </summary>
        /// <param name="refundPaymentRequest">Request</param>
        /// <returns>Result</returns>
        public RefundPaymentResult Refund(RefundPaymentRequest refundPaymentRequest)
        {
            return new RefundPaymentResult { Errors = new[] { "Refund method not supported" } };
        }

        /// <summary>
        /// Voids a payment
        /// </summary>
        /// <param name="voidPaymentRequest">Request</param>
        /// <returns>Result</returns>
        public VoidPaymentResult Void(VoidPaymentRequest voidPaymentRequest)
        {
            return new VoidPaymentResult { Errors = new[] { "Void method not supported" } };
        }

        /// <summary>
        /// Process recurring payment
        /// </summary>
        /// <param name="processPaymentRequest">Payment info required for an order processing</param>
        /// <returns>Process payment result</returns>
        public ProcessPaymentResult ProcessRecurringPayment(ProcessPaymentRequest processPaymentRequest)
        {
            return new ProcessPaymentResult { Errors = new[] { "Recurring payment not supported" } };
        }

        /// <summary>
        /// Cancels a recurring payment
        /// </summary>
        /// <param name="cancelPaymentRequest">Request</param>
        /// <returns>Result</returns>
        public CancelRecurringPaymentResult CancelRecurringPayment(CancelRecurringPaymentRequest cancelPaymentRequest)
        {
            return new CancelRecurringPaymentResult { Errors = new[] { "Recurring payment not supported" } };
        }

        /// <summary>
        /// Gets a value indicating whether customers can complete a payment after order is placed but not completed (for redirection payment methods)
        /// </summary>
        /// <param name="order">Order</param>
        /// <returns>Result</returns>
        public bool CanRePostProcessPayment(Order order)
        {
            if (order == null)
                throw new ArgumentNullException(nameof(order));
            
            //let's ensure that at least 5 seconds passed after order is placed
            //P.S. there's no any particular reason for that. we just do it
            if ((DateTime.UtcNow - order.CreatedOnUtc).TotalSeconds < 5)
                return false;

            return true;
        }

        public override string GetConfigurationPageUrl()
        {
            return $"{_webHelper.GetStoreLocation()}Admin/QualpayCheckout/Configure";
        }

        public void GetPublicViewComponent(out string viewComponentName)
        {
            viewComponentName = "QualpayCheckout";
        }
        
        public IList<string> ValidatePaymentForm(IFormCollection form)
        {
            return new List<string>();
        }

        public ProcessPaymentRequest GetPaymentInfo(IFormCollection form)
        {
            return new ProcessPaymentRequest();
        }

        /// <summary>
        /// Get type of the controller
        /// </summary>
        /// <returns>Controller type</returns>
        public Type GetControllerType()
        {
            return typeof(QualpayCheckoutController);
        }

        /// <summary>
        /// Install the plugin
        /// </summary>
        public override void Install()
        {
            //settings
            _settingService.SaveSetting(new QualpayCheckoutSettings
            {
                UseSandbox = true
            });

            //locales
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.QualpayCheckout.Fields.AdditionalFee", "Additional fee");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.QualpayCheckout.Fields.AdditionalFee.Hint", "Enter additional fee to charge your customers.");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.QualpayCheckout.Fields.AdditionalFeePercentage", "Additional fee. Use percentage");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.QualpayCheckout.Fields.AdditionalFeePercentage.Hint", "Determines whether to apply a percentage additional fee to the order total. If not enabled, a fixed value is used.");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.QualpayCheckout.Fields.EnableEmailReceipts", "Email receipts");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.QualpayCheckout.Fields.EnableEmailReceipts.Hint", "Check for sending the transaction receipts from Qualpay to the customers.");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.QualpayCheckout.Fields.MerchantId", "Merchant ID");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.QualpayCheckout.Fields.MerchantId.Hint", "Specify your Qualpay merchant identifier.");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.QualpayCheckout.Fields.SecurityKey", "Security key");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.QualpayCheckout.Fields.SecurityKey.Hint", "Specify your Qualpay Checkout security key.");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.QualpayCheckout.Fields.UseSandbox", "Use Sandbox");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.QualpayCheckout.Fields.UseSandbox.Hint", "Check to enable sandbox (testing environment).");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.QualpayCheckout.PaymentMethodDescription", "You will be redirected to Qualpay Checkout to complete the payment");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.QualpayCheckout.RedirectionTip", "You will be redirected to Qualpay Checkout to complete the order.");
            
            

            base.Install();
        }

        /// <summary>
        /// Uninstall the plugin
        /// </summary>
        public override void Uninstall()
        {
            //settings
            _settingService.DeleteSetting<QualpayCheckoutSettings>();

            //locales
            this.DeletePluginLocaleResource("Plugins.Payments.QualpayCheckout.Fields.AdditionalFee");
            this.DeletePluginLocaleResource("Plugins.Payments.QualpayCheckout.Fields.AdditionalFee.Hint");
            this.DeletePluginLocaleResource("Plugins.Payments.QualpayCheckout.Fields.AdditionalFeePercentage");
            this.DeletePluginLocaleResource("Plugins.Payments.QualpayCheckout.Fields.AdditionalFeePercentage.Hint");
            this.DeletePluginLocaleResource("Plugins.Payments.QualpayCheckout.Fields.EnableEmailReceipts");
            this.DeletePluginLocaleResource("Plugins.Payments.QualpayCheckout.Fields.EnableEmailReceipts.Hint");
            this.DeletePluginLocaleResource("Plugins.Payments.QualpayCheckout.Fields.MerchantId");
            this.DeletePluginLocaleResource("Plugins.Payments.QualpayCheckout.Fields.MerchantId.Hint");
            this.DeletePluginLocaleResource("Plugins.Payments.QualpayCheckout.Fields.SecurityKey");
            this.DeletePluginLocaleResource("Plugins.Payments.QualpayCheckout.Fields.SecurityKey.Hint");
            this.DeletePluginLocaleResource("Plugins.Payments.QualpayCheckout.Fields.UseSandbox");
            this.DeletePluginLocaleResource("Plugins.Payments.QualpayCheckout.Fields.UseSandbox.Hint");
            this.DeletePluginLocaleResource("Plugins.Payments.QualpayCheckout.PaymentMethodDescription");
            this.DeletePluginLocaleResource("Plugins.Payments.QualpayCheckout.RedirectionTip");

            base.Uninstall();
        }

        #endregion

        #region Properies

        /// <summary>
        /// Gets a value indicating whether capture is supported
        /// </summary>
        public bool SupportCapture
        {
            get { return false; }
        }

        /// <summary>
        /// Gets a value indicating whether partial refund is supported
        /// </summary>
        public bool SupportPartiallyRefund
        {
            get { return false; }
        }

        /// <summary>
        /// Gets a value indicating whether refund is supported
        /// </summary>
        public bool SupportRefund
        {
            get { return false; }
        }

        /// <summary>
        /// Gets a value indicating whether void is supported
        /// </summary>
        public bool SupportVoid
        {
            get { return false; }
        }

        /// <summary>
        /// Gets a recurring payment type of payment method
        /// </summary>
        public RecurringPaymentType RecurringPaymentType
        {
            get { return RecurringPaymentType.NotSupported; }
        }

        /// <summary>
        /// Gets a payment method type
        /// </summary>
        public PaymentMethodType PaymentMethodType
        {
            get { return PaymentMethodType.Redirection; }
        }

        /// <summary>
        /// Gets a value indicating whether we should display a payment information page for this plugin
        /// </summary>
        public bool SkipPaymentInfo
        {
            get { return false; }
        }
        
        /// <summary>
        /// Gets a payment method description that will be displayed on checkout pages in the public store
        /// </summary>
        public string PaymentMethodDescription
        {
            //return description of this payment method to be display on "payment method" checkout step. good practice is to make it localizable
            //for example, for a redirection payment method, description may be like this: "You will be redirected to PayPal site to complete the payment"
            get { return _localizationService.GetResource("Plugins.Payments.QualpayCheckout.PaymentMethodDescription"); }
        }

        #endregion
    }
}
