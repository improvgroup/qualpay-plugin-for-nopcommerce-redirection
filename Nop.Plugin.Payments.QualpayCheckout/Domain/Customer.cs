using Newtonsoft.Json;

namespace Nop.Plugin.Payments.QualpayCheckout.Domain
{
    /// <summary>
    /// Represents the customer information 
    /// </summary>
    public class Customer
    {
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
    }
}