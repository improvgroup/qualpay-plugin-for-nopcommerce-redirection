using System;
using Newtonsoft.Json;

namespace Nop.Plugin.Payments.QualpayCheckout.Domain
{
    /// <summary>
    /// Represents Qualpay Checkout response details
    /// </summary>
    public class QualpayCheckoutResponseDetails
    {
        /// <summary>
        /// Gets or sets a unique identifier generated when a checkout resource is created. 
        /// </summary>
        [JsonProperty(PropertyName = "checkout_id")]
        public string CheckoutId { get; set; }

        /// <summary>
        /// Gets or sets a URL that will direct the customer to the Qualpay Checkout page.
        /// </summary>
        [JsonProperty(PropertyName = "checkout_link")]
        public string CheckoutLink { get; set; }

        /// <summary>
        /// Gets or sets the transaction amount from the request message. 
        /// </summary>
        [JsonProperty(PropertyName = "amt_tran")]
        public string Amount { get; set; }

        /// <summary>
        /// Gets or sets the ISO numeric currency code for the transaction. 
        /// </summary>
        [JsonProperty(PropertyName = "tran_currency")]
        public string CurrencyIsoCode { get; set; }

        /// <summary>
        /// Gets or sets a purchase identifier (or invoice number) included in the request. 
        /// </summary>
        [JsonProperty(PropertyName = "purchase_id")]
        public string PurchaseId { get; set; }

        /// <summary>
        /// Gets or sets the unique profile ID included in the request. 
        /// </summary>
        [JsonProperty(PropertyName = "profile_id")]
        public string ProfileId { get; set; }

        /// <summary>
        /// Gets or sets the customer first name included in the request.
        /// </summary>
        [JsonProperty(PropertyName = "customer_first_name")]
        public string CustomerFirstName { get; set; }

        /// <summary>
        /// Gets or sets the customer last name included in the request..
        /// </summary>
        [JsonProperty(PropertyName = "customer_last_name")]
        public string CustomerLastName { get; set; }

        /// <summary>
        /// Gets or sets the customer email address included in the request.. 
        /// </summary>
        [JsonProperty(PropertyName = "customer_email")]
        public string CustomerEmail { get; set; }

        /// <summary>
        /// Gets or sets the customer phone number.
        /// </summary>
        [JsonProperty(PropertyName = "customer_phone")]
        public string CustomerPhone { get; set; }

        /// <summary>
        /// Gets or sets the billing address of the cardholder.
        /// </summary>
        [JsonProperty(PropertyName = "billing_addr1")]
        public string BllingAddress { get; set; }

        /// <summary>
        /// Gets or sets the billing state of the cardholder. 
        /// </summary>
        [JsonProperty(PropertyName = "billing_state")]
        public string BllingState { get; set; }

        /// <summary>
        /// Gets or sets the billing city of the cardholder. 
        /// </summary>
        [JsonProperty(PropertyName = "billing_city")]
        public string BllingCity { get; set; }

        /// <summary>
        /// Gets or sets the billing zip of the cardholder. 
        /// </summary>
        [JsonProperty(PropertyName = "billing_zip")]
        public string BllingZip { get; set; }

        /// <summary>
        /// Gets or sets the request time stamp.
        /// </summary>
        [JsonProperty(PropertyName = "db_timestamp")]
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// Gets or sets the timestamp which indicates when the checkout link will expire. A link will expire once the complete payment is made or if the current date is past the expiration time.  
        /// </summary>
        [JsonProperty(PropertyName = "expiry_time")]
        public DateTime ExpirationTime { get; set; }

        /// <summary>
        /// Gets or sets the preferences included when creating the resource. 
        /// </summary>
        [JsonProperty(PropertyName = "preferences")]
        public Preferences Preferences { get; set; }

        /// <summary>
        /// Gets or sets a detailed list of the transactions charged against this checkout.
        /// </summary>
        [JsonProperty(PropertyName = "transactions")]
        public Transaction[] Transactions { get; set; }
    }
}