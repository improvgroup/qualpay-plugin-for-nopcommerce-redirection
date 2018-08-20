using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Nop.Core;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Orders;
using Nop.Core.Plugins;
using Nop.Plugin.Payments.QualpayCheckout.Domain;
using Nop.Plugin.Payments.QualpayCheckout.Services;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Payments;

namespace Nop.Plugin.Payments.QualpayCheckout
{
    /// <summary>
    /// Represents Qualpay Checkout processor
    /// </summary>
    public class QualpayCheckoutProcessor : BasePlugin, IPaymentMethod
    {
        #region Fields

        private readonly CurrencySettings _currencySettings;
        private readonly ICurrencyService _currencyService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILocalizationService _localizationService;
        private readonly IPaymentService _paymentService;
        private readonly ISettingService _settingService;
        private readonly IWebHelper _webHelper;
        private readonly QualpayCheckoutManager _qualpayCheckoutManager;
        private readonly QualpayCheckoutSettings _qualpayCheckoutSettings;

        #endregion

        #region Ctor

        public QualpayCheckoutProcessor(CurrencySettings currencySettings,
            ICurrencyService currencyService,
            IGenericAttributeService genericAttributeService,
            IHttpContextAccessor httpContextAccessor,
            ILocalizationService localizationService,
            IPaymentService paymentService,
            ISettingService settingService,
            IWebHelper webHelper,
            QualpayCheckoutManager qualpayCheckoutManager,
            QualpayCheckoutSettings qualpayCheckoutSettings)
        {
            this._currencySettings = currencySettings;
            this._currencyService = currencyService;
            this._genericAttributeService = genericAttributeService;
            this._httpContextAccessor = httpContextAccessor;
            this._localizationService = localizationService;
            this._paymentService = paymentService;
            this._settingService = settingService;
            this._webHelper = webHelper;
            this._qualpayCheckoutManager = qualpayCheckoutManager;
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
            //Qualpay Checkout supports only USD currency
            var primaryStoreCurrency = _currencyService.GetCurrencyById(_currencySettings.PrimaryStoreCurrencyId);
            if (!primaryStoreCurrency.CurrencyCode.Equals("USD", StringComparison.InvariantCultureIgnoreCase))
                throw new NopException("USD is not a primary store currency");
            
            //store location
            var storeLocation = _webHelper.GetStoreLocation();

            //create checkout request
            var checkoutRequest = new QualpayCheckoutRequest
            {
                Amount = Math.Round(postProcessPaymentRequest.Order.OrderTotal, 2),
                CurrencyIsoCode = QualpayCheckoutDefaults.UsdNumericIsoCode,
                PurchaseId = postProcessPaymentRequest.Order.CustomOrderNumber,
                CustomerFirstName = postProcessPaymentRequest.Order.BillingAddress?.FirstName,
                CustomerLastName = postProcessPaymentRequest.Order.BillingAddress?.LastName,
                CustomerEmail = postProcessPaymentRequest.Order.BillingAddress?.Email,
                BllingAddress = CommonHelper.EnsureMaximumLength(postProcessPaymentRequest.Order.BillingAddress?.Address1, 20),
                BllingCity = postProcessPaymentRequest.Order.BillingAddress?.City,
                BllingState = postProcessPaymentRequest.Order.BillingAddress?.StateProvince?.Abbreviation,
                BllingZip = postProcessPaymentRequest.Order.BillingAddress?.ZipPostalCode,
                Preferences = new Preferences
                {
                    AllowPartialPayments = false,
                    EnableEmailReceipts = _qualpayCheckoutSettings.EnableEmailReceipts,
                    ExpirationTime = 1200,
                    RequestType = RequestType.Sale,
                    SuccessUrl = $"{storeLocation}checkout/completed/{postProcessPaymentRequest.Order.Id}",
                    FailureUrl = $"{storeLocation}orderdetails/{postProcessPaymentRequest.Order.Id}",
                    NotificationUrl = $"{storeLocation}Plugins/QualpayCheckout/IPN"
                }
            };

            //get checkout link
            var redirectUrl = string.Empty;
            var checkoutResponse = _qualpayCheckoutManager.Checkout(checkoutRequest);
            if (checkoutResponse != null)
            {
                //save chekout id for the further validation
                _genericAttributeService
                    .SaveAttribute(postProcessPaymentRequest.Order, QualpayCheckoutDefaults.CheckoutIdAttribute, checkoutResponse.CheckoutId);

                //redirect to Qualpay Checkout
                redirectUrl = checkoutResponse.CheckoutLink;
            }
            else
                redirectUrl = $"{storeLocation}orderdetails/{postProcessPaymentRequest.Order.Id}";

            _httpContextAccessor.HttpContext.Response.Redirect(redirectUrl);
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
            var result = _paymentService.CalculateAdditionalFee(cart,
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

        /// <summary>
        /// Validate payment form
        /// </summary>
        /// <param name="form">The parsed form values</param>
        /// <returns>List of validating errors</returns>
        public IList<string> ValidatePaymentForm(IFormCollection form)
        {
            return new List<string>();
        }

        /// <summary>
        /// Get payment information
        /// </summary>
        /// <param name="form">The parsed form values</param>
        /// <returns>Payment info holder</returns>
        public ProcessPaymentRequest GetPaymentInfo(IFormCollection form)
        {
            return new ProcessPaymentRequest();
        }

        /// <summary>
        /// Gets a configuration page URL
        /// </summary>
        public override string GetConfigurationPageUrl()
        {
            return $"{_webHelper.GetStoreLocation()}Admin/QualpayCheckout/Configure";
        }

        /// <summary>
        /// Gets a view component for displaying plugin in public store ("payment info" checkout step)
        /// </summary>
        /// <param name="viewComponentName">View component name</param>
        public string GetPublicViewComponentName()
        {
            return QualpayCheckoutDefaults.ViewComponentName;
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
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.QualpayCheckout.Fields.AdditionalFee", "Additional fee");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.QualpayCheckout.Fields.AdditionalFee.Hint", "Enter additional fee to charge your customers.");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.QualpayCheckout.Fields.AdditionalFeePercentage", "Additional fee. Use percentage");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.QualpayCheckout.Fields.AdditionalFeePercentage.Hint", "Determines whether to apply a percentage additional fee to the order total. If not enabled, a fixed value is used.");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.QualpayCheckout.Fields.EnableEmailReceipts", "Email receipts");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.QualpayCheckout.Fields.EnableEmailReceipts.Hint", "Check for sending the transaction receipts from Qualpay to the customers.");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.QualpayCheckout.Fields.MerchantEmail", "Email");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.QualpayCheckout.Fields.MerchantEmail.Hint", "Enter your email to subscribe to Qualpay news.");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.QualpayCheckout.Fields.MerchantId", "Merchant ID");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.QualpayCheckout.Fields.MerchantId.Hint", "Specify your Qualpay merchant identifier.");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.QualpayCheckout.Fields.SecurityKey", "Security key");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.QualpayCheckout.Fields.SecurityKey.Hint", "Specify your Qualpay Checkout security key.");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.QualpayCheckout.Fields.UseSandbox", "Use Sandbox");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.QualpayCheckout.Fields.UseSandbox.Hint", "Check to enable sandbox (testing environment).");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.QualpayCheckout.PaymentMethodDescription", "You will be redirected to Qualpay Checkout to complete the payment");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.QualpayCheckout.RedirectionTip", "You will be redirected to Qualpay Checkout to complete the order.");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.QualpayCheckout.Subscribe", "Stay informed");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.QualpayCheckout.Subscribe.Error", "An error has occurred, details in the log");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.QualpayCheckout.Subscribe.Success", "You have subscribed to Qualpay news");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.QualpayCheckout.Unsubscribe.Success", "You have unsubscribed from Qualpay news");

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
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.QualpayCheckout.Fields.AdditionalFee");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.QualpayCheckout.Fields.AdditionalFee.Hint");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.QualpayCheckout.Fields.AdditionalFeePercentage");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.QualpayCheckout.Fields.AdditionalFeePercentage.Hint");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.QualpayCheckout.Fields.EnableEmailReceipts");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.QualpayCheckout.Fields.EnableEmailReceipts.Hint");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.QualpayCheckout.Fields.MerchantEmail");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.QualpayCheckout.Fields.MerchantEmail.Hint");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.QualpayCheckout.Fields.MerchantId");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.QualpayCheckout.Fields.MerchantId.Hint");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.QualpayCheckout.Fields.SecurityKey");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.QualpayCheckout.Fields.SecurityKey.Hint");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.QualpayCheckout.Fields.UseSandbox");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.QualpayCheckout.Fields.UseSandbox.Hint");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.QualpayCheckout.PaymentMethodDescription");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.QualpayCheckout.RedirectionTip");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.QualpayCheckout.Subscribe");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.QualpayCheckout.Subscribe.Error");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.QualpayCheckout.Subscribe.Success");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.QualpayCheckout.Unsubscribe.Success");

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