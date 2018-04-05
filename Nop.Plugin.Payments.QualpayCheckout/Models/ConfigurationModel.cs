using System.ComponentModel.DataAnnotations;
using Nop.Web.Framework.Mvc.ModelBinding;
using Nop.Web.Framework.Mvc.Models;

namespace Nop.Plugin.Payments.QualpayCheckout.Models
{
    /// <summary>
    /// Represents the Qualpay Checkout configuration model
    /// </summary>
    public class ConfigurationModel : BaseNopModel
    {
        #region Properties

        public int ActiveStoreScopeConfiguration { get; set; }

        [NopResourceDisplayName("Plugins.Payments.QualpayCheckout.Fields.MerchantId")]
        public string MerchantId { get; set; }

        [NopResourceDisplayName("Plugins.Payments.QualpayCheckout.Fields.SecurityKey")]
        [DataType(DataType.Password)]
        [NoTrim]
        public string SecurityKey { get; set; }
        public bool SecurityKey_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Payments.QualpayCheckout.Fields.EnableEmailReceipts")]
        public bool EnableEmailReceipts { get; set; }
        public bool EnableEmailReceipts_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Payments.QualpayCheckout.Fields.UseSandbox")]
        public bool UseSandbox { get; set; }
        public bool UseSandbox_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Payments.QualpayCheckout.Fields.AdditionalFee")]
        public decimal AdditionalFee { get; set; }
        public bool AdditionalFee_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Payments.QualpayCheckout.Fields.AdditionalFeePercentage")]
        public bool AdditionalFeePercentage { get; set; }
        public bool AdditionalFeePercentage_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Payments.QualpayCheckout.Fields.MerchantEmail")]
        [DataType(DataType.EmailAddress)]
        public string MerchantEmail { get; set; }

        #endregion
    }
}