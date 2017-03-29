using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

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

    /// <summary>
    /// Represents a single transaction made through Qualpay Checkout.
    /// </summary>
    public class Transaction
    {
        /// <summary>
        /// Gets or sets a unique identifier generated when a checkout resource is created. 
        /// </summary>
        [JsonProperty(PropertyName = "checkout_id")]
        public string CheckoutId { get; set; }

        /// <summary>
        /// Gets or sets a unique identifier generated by the payment gateway for each transaction. 
        /// </summary>
        [JsonProperty(PropertyName = "pg_id")]
        public string TransactionId { get; set; }

        /// <summary>
        /// Gets or sets the response code from the Qualpay payment gateway
        /// </summary>
        [JsonProperty(PropertyName = "rcode")]
        public string ResponseCode { get; set; }

        /// <summary>
        /// Gets or sets a text description of the authorization response code from Qualpay payment gateway 
        /// </summary>
        [JsonProperty(PropertyName = "rmsg")]
        public string ResponseMessage { get; set; }

        /// <summary>
        /// Gets or sets a status of transaction
        /// </summary>
        [JsonProperty(PropertyName = "status")]
        public string Status { get; set; }

        /// <summary>
        /// Gets or sets the date timestamp when the transaction was submitted.
        /// </summary>
        [JsonProperty(PropertyName = "tran_time")]
        public DateTime TransactionTime { get; set; }

        /// <summary>
        /// Gets or sets a masked card number.
        /// </summary>
        [JsonProperty(PropertyName = "card_number")]
        public string CardNumber { get; set; }

        /// <summary>
        /// Gets or sets the transaction amount of this transaction.
        /// </summary>
        [JsonProperty(PropertyName = "amt_tran")]
        public decimal Amount { get; set; }

        /// <summary>
        /// Gets or sets the ISO numeric currency code for the transaction. 
        /// </summary>
        [JsonProperty(PropertyName = "tran_currency")]
        public int CurrencyIsoCode { get; set; }

        /// <summary>
        /// Gets or sets the currency code of the transaction. 
        /// </summary>
        [JsonProperty(PropertyName = "currencyAlpha")]
        public string CurrencyAlpha { get; set; }

        /// <summary>
        /// Gets or sets a purchase identifier (or invoice number) for the transaction. 
        /// </summary>
        [JsonProperty(PropertyName = "purchase_id")]
        public string PurchaseId { get; set; }

        /// <summary>
        /// Gets or sets the unique profile ID used to submit the transaction. 
        /// </summary>
        [JsonProperty(PropertyName = "profile_id")]
        public string ProfileId { get; set; }

        /// <summary>
        /// Gets or sets the authorization code returned when the transaction was sent to the card issuer for approval.
        /// </summary>
        [JsonProperty(PropertyName = "auth_code")]
        public string AuthorizationCode { get; set; }

        /// <summary>
        /// Gets or sets the result from the card issuer.
        /// </summary>
        [JsonProperty(PropertyName = "cvv2_result")]
        public string Cvv2Result { get; set; }

        /// <summary>
        /// Gets or sets the result from the card issuer.
        /// </summary>
        [JsonProperty(PropertyName = "avs_result")]
        public string AvsResult { get; set; }

        /// <summary>
        /// Gets or sets the Qualpay Account Identifier.
        /// </summary>
        [JsonProperty(PropertyName = "merchant_id")]
        public string MerchantId { get; set; }

        /// <summary>
        /// Gets or sets the IP address of the device initiating the payment.
        /// </summary>
        [JsonProperty(PropertyName = "client_ip")]
        public string CustomerIpAddress { get; set; }

        /// <summary>
        /// Gets or sets the DBA name associated with the profile.
        /// </summary>
        [JsonProperty(PropertyName = "dba_name")]
        public string DbaName { get; set; }

        /// <summary>
        /// Gets or sets the customer information submitted with the transaction.
        /// </summary>
        [JsonProperty(PropertyName = "customer")]
        public Customer Customer { get; set; }
    }

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
