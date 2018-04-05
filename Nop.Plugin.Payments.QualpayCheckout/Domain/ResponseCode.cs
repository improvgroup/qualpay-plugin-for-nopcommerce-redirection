
namespace Nop.Plugin.Payments.QualpayCheckout.Domain
{
    /// <summary>
    /// Represents enumeration of Qualpay API Response Codes
    /// </summary>
    public enum ResponseCode
    {
        /// <summary>
        /// The request was successful.
        /// </summary>
        OK = 0,

        /// <summary>
        /// The request failed validation. Check the data element for a more detailed list of validation failures.
        /// </summary>
        BadRequest = 2,

        /// <summary>
        /// The API Key being used does not have access to this resource. Access may be granted through the Qualpay Manager.
        /// </summary>
        Forbidden = 6,

        /// <summary>
        /// The service you have called does not exist. Please check and verify the resource URL for your request.
        /// </summary>
        NotFound = 7,

        /// <summary>
        /// The request was missing required HTTP Basic Authentication, or the credentials provided were invalid
        /// </summary>
        Unauthorized = 11,

        /// <summary>
        /// There was a server problem processing the request. Qualpay developers are notified automatically of the issue when this API Code occurs.
        /// </summary>
        InternalServerError = 99
    }
}