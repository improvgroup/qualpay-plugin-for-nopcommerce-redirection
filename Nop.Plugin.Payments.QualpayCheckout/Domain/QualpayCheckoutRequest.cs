using Newtonsoft.Json;

namespace Nop.Plugin.Payments.QualpayCheckout.Domain
{
    /// <summary>
    /// Represents Qualpay Checkout request
    /// </summary>
    public class QualpayCheckoutRequest
    {
        /// <summary>
        /// Gets or sets the total amount of the transaction including sales tax (if applicable).
        /// </summary>
        [JsonProperty(PropertyName = "amt_tran")]
        public decimal Amount { get; set; }

        /// <summary>
        /// Gets or sets the ISO numeric currency code for the transaction. 
        /// </summary>
        [JsonProperty(PropertyName = "tran_currency")]
        public int CurrencyIsoCode { get; set; }

        /// <summary>
        /// Gets or sets a purchase identifier (or invoice number) that will be stored with the transaction data and will be included with the transaction data reported in the Qualpay Manager. 
        /// </summary>
        [JsonProperty(PropertyName = "purchase_id")]
        public string PurchaseId { get; set; }

        /// <summary>
        /// Gets or sets the unique profile ID to be used in payment gateway requests. Use this if you have multiple profiles for the same currency or if the request should be processed using specific profile_id. 
        /// </summary>
        [JsonProperty(PropertyName = "profile_id")]
        public string ProfileId { get; set; }

        /// <summary>
        /// Gets or sets a reference value that will be stored with the transaction data and will be included with the transaction data reported in the Qualpay Manager.
        /// </summary>
        [JsonProperty(PropertyName = "merch_ref_num")]
        public string MerchantReferenceInfo { get; set; }

        /// <summary>
        /// Gets or sets settings to override any preferences that have been set in the Qualpay Manager Checkout settings page
        /// </summary>
        [JsonProperty(PropertyName = "preferences")]
        public Preferences Preferences { get; set; }

        /// <summary>
        /// Gets or sets the customer first name.
        /// </summary>
        [JsonProperty(PropertyName = "customer_first_name")]
        public string CustomerFirstName { get; set; }

        /// <summary>
        /// Gets or sets the customer last name.
        /// </summary>
        [JsonProperty(PropertyName = "customer_last_name")]
        public string CustomerLastName { get; set; }

        /// <summary>
        /// Gets or sets the customer email address. 
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
    }
}