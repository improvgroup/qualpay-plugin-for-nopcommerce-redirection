using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Plugin.Payments.QualpayCheckout.Models;
using Nop.Plugin.Payments.QualpayCheckout.Services;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Security;
using Nop.Web.Areas.Admin.Controllers;
using Nop.Web.Framework.Controllers;

namespace Nop.Plugin.Payments.QualpayCheckout.Controllers
{
    public class QualpayCheckoutController : BaseAdminController
    {
        #region Fields

        private readonly ILocalizationService _localizationService;
        private readonly IPermissionService _permissionService;
        private readonly ISettingService _settingService;
        private readonly IStoreContext _storeContext;
        private readonly QualpayCheckoutManager _qualpayCheckoutManager;

        #endregion

        #region Ctor

        public QualpayCheckoutController(ILocalizationService localizationService,
            IPermissionService permissionService,
            ISettingService settingService,
            IStoreContext storeContext,
            QualpayCheckoutManager qualpayCheckoutManager)
        {
            this._localizationService = localizationService;
            this._permissionService = permissionService;
            this._settingService = settingService;
            this._storeContext = storeContext;
            this._qualpayCheckoutManager = qualpayCheckoutManager;
        }

        #endregion

        #region Methods

        public IActionResult Configure()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePaymentMethods))
                return AccessDeniedView();

            //load settings for a chosen store scope
            var storeScope = _storeContext.ActiveStoreScopeConfiguration;
            var settings = _settingService.LoadSetting<QualpayCheckoutSettings>(storeScope);

            //prepare model
            var model = new ConfigurationModel
            {
                MerchantId = settings.MerchantId,
                MerchantEmail = settings.MerchantEmail,
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

        [HttpPost, ActionName("Configure")]
        [FormValueRequired("save")]
        public IActionResult Configure(ConfigurationModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePaymentMethods))
                return AccessDeniedView();

            if (!ModelState.IsValid)
                return Configure();

            //load settings for a chosen store scope
            var storeScope = _storeContext.ActiveStoreScopeConfiguration;
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

        [HttpPost, ActionName("Configure")]
        [FormValueRequired("subscribe")]
        public IActionResult Subscribe(ConfigurationModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePaymentMethods))
                return AccessDeniedView();

            //load settings
            var settings = _settingService.LoadSetting<QualpayCheckoutSettings>();
            if (settings.MerchantEmail == model.MerchantEmail)
                return Configure();

            //try to subscribe/unsubscribe
            var successfullySubscribed = _qualpayCheckoutManager.SubscribeToQualpay(model.MerchantEmail);
            if (successfullySubscribed)
            {
                //save settings and display success notification
                settings.MerchantEmail = model.MerchantEmail;
                _settingService.SaveSetting(settings);

                var message = !string.IsNullOrEmpty(model.MerchantEmail)
                    ? _localizationService.GetResource("Plugins.Payments.QualpayCheckout.Subscribe.Success")
                    : _localizationService.GetResource("Plugins.Payments.QualpayCheckout.Unsubscribe.Success");
                SuccessNotification(message);
            }
            else
                ErrorNotification("Plugins.Payments.QualpayCheckout.Subscribe.Error");

            return Configure();
        }

        #endregion
    }
}