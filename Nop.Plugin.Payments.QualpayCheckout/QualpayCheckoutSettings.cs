using Nop.Core.Configuration;

namespace Nop.Plugin.Payments.QualpayCheckout
{
    /// <summary>
    /// Represents Qualpay Checkout settings
    /// </summary>
    public class QualpayCheckoutSettings : ISettings
    {
        /// <summary>
        /// Gets or sets Qualpay merchant identifier
        /// </summary>
        public string MerchantId { get; set; }

        /// <summary>
        /// Gets or sets Qualpay Checkout security key
        /// </summary>
        public string SecurityKey { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether transaction receipts from Qualpay will be emailed to the customers
        /// </summary>
        public bool EnableEmailReceipts { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to use sandbox (testing environment)
        /// </summary>
        public bool UseSandbox { get; set; }

        /// <summary>
        /// Gets or sets an additional fee
        /// </summary>
        public decimal AdditionalFee { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to "additional fee" is specified as percentage. true - percentage, false - fixed value.
        /// </summary>
        public bool AdditionalFeePercentage { get; set; }
    }
}
