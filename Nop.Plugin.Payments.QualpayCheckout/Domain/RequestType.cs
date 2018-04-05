using Newtonsoft.Json;

namespace Nop.Plugin.Payments.QualpayCheckout.Domain
{
    /// <summary>
    /// Represents enumeration of available Qualpay Checkout request type
    /// </summary>
    public enum RequestType
    {
        /// <summary>
        /// Authorization only
        /// </summary>
        [JsonProperty(PropertyName = "auth")]
        Authorization,

        /// <summary>
        /// Authorize and capture
        /// </summary>
        [JsonProperty(PropertyName = "sale")]
        Sale
    }
}