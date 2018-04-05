using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Nop.Plugin.Payments.QualpayCheckout.Domain
{
    /// <summary>
    /// Represents Qualpay Checkout preferences
    /// </summary>
    public class Preferences
    {
        /// <summary>
        /// Gets or sets a URL to which the customer will be directed to after a successful transaction. 
        /// </summary>
        [JsonProperty(PropertyName = "success_url")]
        public string SuccessUrl { get; set; }

        /// <summary>
        /// Gets or sets a URL to which the customer will be directed after a failed transaction. 
        /// </summary>
        [JsonProperty(PropertyName = "failure_url")]
        public string FailureUrl { get; set; }

        /// <summary>
        /// Gets or sets a URL that will be notified whenever a checkout transaction is attempted.
        /// </summary>
        [JsonProperty(PropertyName = "notification_url")]
        public string NotificationUrl { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether a customer can edit the transaction amount on Qualpay Checkout. 
        /// </summary>
        [JsonProperty(PropertyName = "allow_partial_payments")]
        public bool AllowPartialPayments { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether transaction receipts will be emailed to the customers.  
        /// </summary>
        [JsonProperty(PropertyName = "email_receipt")]
        public bool EnableEmailReceipts { get; set; }

        /// <summary>
        /// Gets or sets the type of request when the customer submits the payment data on the checkout page.
        /// </summary>
        [JsonProperty(PropertyName = "request_type")]
        [JsonConverter(typeof(StringEnumConverter))]
        public RequestType RequestType { get; set; }

        /// <summary>
        /// Gets or sets the time period for which the checkout link will be valid in seconds. 
        /// </summary>
        [JsonProperty(PropertyName = "expire_in_secs")]
        public int ExpirationTime { get; set; }
    }
}