using Newtonsoft.Json;

namespace Nop.Plugin.Payments.QualpayCheckout.Domain
{
    /// <summary>
    /// Represents Qualpay Checkout response
    /// </summary>
    public class QualpayCheckoutResponse
    {
        /// <summary>
        /// Gets or sets the response code
        /// </summary>
        [JsonProperty(PropertyName = "code")]
        public ResponseCode ResponseCode { get; set; }

        /// <summary>
        /// Gets or sets the response message
        /// </summary>
        [JsonProperty(PropertyName = "message")]
        public string Message { get; set; }

        /// <summary>
        /// Gets or sets the response details
        /// </summary>
        [JsonProperty(PropertyName = "data")]
        public QualpayCheckoutResponseDetails Details { get; set; }
    }
}