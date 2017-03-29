using System;
using System.Collections.Generic;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Plugin.Payments.QualpayCheckout.Helpers;
using Nop.Plugin.Payments.QualpayCheckout.Models;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Stores;
using Nop.Web.Framework.Controllers;

namespace Nop.Plugin.Payments.QualpayCheckout.Controllers
{
    public class QualpayCheckoutController : BasePaymentController
    {
        #region Fields

        private readonly HttpContextBase _httpContext;
        private readonly ICurrencyService _currencyService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly ILocalizationService _localizationService;
        private readonly ILogger _logger;
        private readonly IOrderProcessingService _orderProcessingService;
        private readonly IOrderService _orderService;
        private readonly ISettingService _settingService;
        private readonly IStoreService _storeService;
        private readonly IWorkContext _workContext;

        #endregion

        #region Ctor

        public QualpayCheckoutController(HttpContextBase httpContext,
            ICurrencyService currencyService,
            IGenericAttributeService genericAttributeService,
            ILocalizationService localizationService,
            ILogger logger,
            IOrderProcessingService orderProcessingService,
            IOrderService orderService,
            ISettingService settingService,
            IStoreService storeService,
            IWorkContext workContext)
        {
            this._httpContext = httpContext;
            this._currencyService = currencyService;
            this._genericAttributeService = genericAttributeService;
            this._localizationService = localizationService;
            this._logger = logger;
            this._orderProcessingService = orderProcessingService;
            this._orderService = orderService;
            this._settingService = settingService;
            this._storeService = storeService;
            this._workContext = workContext;
        }

        #endregion

        #region Methods

        [AdminAuthorize]
        [ChildActionOnly]
        public ActionResult Configure()
        {
            //load settings for a chosen store scope
            var storeScope = GetActiveStoreScopeConfiguration(_storeService, _workContext);
            var settings = _settingService.LoadSetting<QualpayCheckoutSettings>(storeScope);

            //prepare model
            var model = new ConfigurationModel
            {
                MerchantId = settings.MerchantId,
                SecurityKey = settings.SecurityKey,
                EnableEmailReceipts = settings.EnableEmailReceipts,
                UseSandbox = settings.UseSandbox,
                AdditionalFee = settings.AdditionalFee,
                AdditionalFeePercentage = settings.AdditionalFeePercentage,
                ActiveStoreScopeConfiguration = storeScope
            };

            if (storeScope > 0)
            {
                model.SecurityKey_OverrideForStore = _settingService.SettingExists(settings, x => x.SecurityKey, storeScope);
                model.EnableEmailReceipts_OverrideForStore = _settingService.SettingExists(settings, x => x.EnableEmailReceipts, storeScope);
                model.UseSandbox_OverrideForStore = _settingService.SettingExists(settings, x => x.UseSandbox, storeScope);
                model.AdditionalFee_OverrideForStore = _settingService.SettingExists(settings, x => x.AdditionalFee, storeScope);
                model.AdditionalFeePercentage_OverrideForStore = _settingService.SettingExists(settings, x => x.AdditionalFeePercentage, storeScope);
            }

            return View("~/Plugins/Payments.QualpayCheckout/Views/Configure.cshtml", model);
        }

        [HttpPost]
        [AdminAuthorize]
        [ChildActionOnly]
        public ActionResult Configure(ConfigurationModel model)
        {
            if (!ModelState.IsValid)
                return Configure();

            //load settings for a chosen store scope
            var storeScope = GetActiveStoreScopeConfiguration(_storeService, _workContext);
            var settings = _settingService.LoadSetting<QualpayCheckoutSettings>(storeScope);

            //save settings
            settings.MerchantId = model.MerchantId;
            settings.SecurityKey = model.SecurityKey;
            settings.EnableEmailReceipts = model.EnableEmailReceipts;
            settings.UseSandbox = model.UseSandbox;
            settings.AdditionalFee = model.AdditionalFee;
            settings.AdditionalFeePercentage = model.AdditionalFeePercentage;

            /* We do not clear cache after each setting update.
             * This behavior can increase performance because cached settings will not be cleared 
             * and loaded from database after each update */
            _settingService.SaveSetting(settings, x => x.MerchantId, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(settings, x => x.SecurityKey, model.SecurityKey_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(settings, x => x.EnableEmailReceipts, model.EnableEmailReceipts_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(settings, x => x.UseSandbox, model.UseSandbox_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(settings, x => x.AdditionalFee, model.AdditionalFee_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(settings, x => x.AdditionalFeePercentage, model.AdditionalFeePercentage_OverrideForStore, storeScope, false);

            //now clear settings cache
            _settingService.ClearCache();

            SuccessNotification(_localizationService.GetResource("Admin.Plugins.Saved"));

            return Configure();
        }

        [ChildActionOnly]
        public ActionResult PaymentInfo()
        {
            return View("~/Plugins/Payments.QualpayCheckout/Views/PaymentInfo.cshtml");
        }

        [NonAction]
        public override IList<string> ValidatePaymentForm(FormCollection form)
        {
            return new List<string>();
        }

        [NonAction]
        public override ProcessPaymentRequest GetPaymentInfo(FormCollection form)
        {
            return new ProcessPaymentRequest();
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult IPNHandler()
        {
            //get Qualpay transaction 
            var transactionString = string.Empty;
            var transaction = QualpayCheckoutHelper.GetTransaction(_httpContext, _logger, out transactionString);
            if (transaction == null)
                return new HttpStatusCodeResult(HttpStatusCode.OK);

            //try to get order for this transaction
            var order = _orderService.GetOrderByCustomOrderNumber(transaction.PurchaseId);
            if (order == null)
            {
                _logger.Error(string.Format("Qualpay Checkout IPN error: order with number {0} not found", transaction.PurchaseId));
                return new HttpStatusCodeResult(HttpStatusCode.OK);
            }

            //validate received transaction by comparing some of data
            var checkoutId = order.GetAttribute<string>("QualpayCheckoutId", _genericAttributeService) ?? string.Empty;
            if (!checkoutId.Equals(transaction.CheckoutId, StringComparison.InvariantCulture))
            {
                _logger.Error(string.Format("Qualpay Checkout IPN error: saved Qualpay checkoutId ({0}) for the order {1} is not match with the received one ({2})", 
                    checkoutId, order.CustomOrderNumber, transaction.CheckoutId));
                return new HttpStatusCodeResult(HttpStatusCode.OK);
            }

            //add order note
            order.OrderNotes.Add(new OrderNote()
            {
                Note = transactionString,
                DisplayToCustomer = false,
                CreatedOnUtc = DateTime.UtcNow
            });
            _orderService.UpdateOrder(order);

            //compare amounts
            var amount = _currencyService.ConvertCurrency(order.OrderTotal, order.CurrencyRate);
            if (Math.Round(amount, 2) != Math.Round(transaction.Amount, 2))
                return new HttpStatusCodeResult(HttpStatusCode.OK);

            //all is ok, so paid order
            if (_orderProcessingService.CanMarkOrderAsPaid(order))
            {
                //set payment details
                order.AuthorizationTransactionCode = transaction.AuthorizationCode;
                order.CaptureTransactionId = transaction.TransactionId;
                order.CaptureTransactionResult = transaction.ResponseMessage;
                _orderService.UpdateOrder(order);

                _orderProcessingService.MarkOrderAsPaid(order);
            }

            //delete validation data
            _genericAttributeService.SaveAttribute<string>(order, "QualpayCheckoutId", null);

            return new HttpStatusCodeResult(HttpStatusCode.OK);
        }

        #endregion
    }
}