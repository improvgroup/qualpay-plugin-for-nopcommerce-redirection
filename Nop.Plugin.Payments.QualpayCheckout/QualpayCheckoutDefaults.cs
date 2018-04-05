
namespace Nop.Plugin.Payments.QualpayCheckout
{
    /// <summary>
    /// Represents Qualpay Checkout payment gateway constants
    /// </summary>
    public class QualpayCheckoutDefaults
    {
        /// <summary>
        /// Qualpay Checkout payment method system name
        /// </summary>
        public static string SystemName => "Payments.QualpayCheckout";

        /// <summary>
        /// User agent using for requesting Qualpay Checkout services
        /// </summary>
        public static string UserAgent => "nopCommerce-checkout-plugin";

        /// <summary>
        /// Name of the view component to display plugin in public store
        /// </summary>
        public const string ViewComponentName = "QualpayCheckout";

        /// <summary>
        /// IPN route name
        /// </summary>
        public static string IpnRouteName => "Plugin.Payments.QualpayCheckout.IPN";

        /// <summary>
        /// Numeric ISO code of the USD currency
        /// </summary>
        public static int UsdNumericIsoCode => 840;

        /// <summary>
        /// Name of the generic attribute to store Qualpay Checkout identifier
        /// </summary>
        public static string CheckoutIdAttribute => "QualpayCheckoutId";

        /// <summary>
        /// Subscription email
        /// </summary>
        public static string SubscriptionEmail => "jgilbert@qualpay.com";
    }
}