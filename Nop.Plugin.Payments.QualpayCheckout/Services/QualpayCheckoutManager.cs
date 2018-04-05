using System;
using System.IO;
using System.Net;
using System.Text;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Nop.Core;
using Nop.Core.Domain.Messages;
using Nop.Plugin.Payments.QualpayCheckout.Domain;
using Nop.Services.Logging;
using Nop.Services.Messages;

namespace Nop.Plugin.Payments.QualpayCheckout.Services
{
    /// <summary>
    /// Represents the Qualpay Checkout manager
    /// </summary>
    public class QualpayCheckoutManager
    {
        #region Fields

        private readonly EmailAccountSettings _emailAccountSettings;
        private readonly IEmailAccountService _emailAccountService;
        private readonly IEmailSender _emailSender;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger _logger;
        private readonly IWorkContext _workContext;
        private readonly QualpayCheckoutSettings _qualpayCheckoutSettings;

        #endregion

        #region Ctor

        public QualpayCheckoutManager(EmailAccountSettings emailAccountSettings,
            IEmailAccountService emailAccountService,
            IEmailSender emailSender,
            IHttpContextAccessor httpContextAccessor,
            ILogger logger,
            IWorkContext workContext,
            QualpayCheckoutSettings qualpayCheckoutSettings)
        {
            this._emailAccountSettings = emailAccountSettings;
            this._emailAccountService = emailAccountService;
            this._emailSender = emailSender;
            this._httpContextAccessor = httpContextAccessor;
            this._logger = logger;
            this._workContext = workContext;
            this._qualpayCheckoutSettings = qualpayCheckoutSettings;
        }

        #endregion

        #region Utilities

        /// <summary>
        /// Gets Qualpay Checkout service URL
        /// </summary>
        /// <returns>URL</returns>
        private string GetQualpayCheckoutServiceUrl()
        {
            return _qualpayCheckoutSettings.UseSandbox ? "https://app-test.qualpay.com/service/api/" : "https://app.qualpay.com/service/api/";
        }

        /// <summary>
        /// Post the request and get a response from Qualpay Checkout service
        /// </summary>
        /// <typeparam name="TRequest">Request type</typeparam>
        /// <typeparam name="TResponse">Response type</typeparam>
        /// <param name="request">Request</param>
        /// <returns>Response</returns>
        private TResponse PostRequest<TRequest, TResponse>(TRequest request)
            where TRequest : QualpayCheckoutRequest where TResponse : QualpayCheckoutResponse
        {
            //ensure that plugin is configured
            if (string.IsNullOrEmpty(_qualpayCheckoutSettings.MerchantId) || string.IsNullOrEmpty(_qualpayCheckoutSettings.SecurityKey))
                throw new NopException("Plugin not configured");

            //create request
            var url = $"{GetQualpayCheckoutServiceUrl()}checkout";
            var webRequest = (HttpWebRequest)WebRequest.Create(url);
            webRequest.Method = WebRequestMethods.Http.Post;
            webRequest.UserAgent = QualpayCheckoutDefaults.UserAgent;
            webRequest.ContentType = "application/json; charset=utf-8";
            webRequest.Accept = "application/json";

            //set authentication
            var login = $"{_qualpayCheckoutSettings.MerchantId}:{_qualpayCheckoutSettings.SecurityKey}";
            var authorization = Convert.ToBase64String(Encoding.UTF8.GetBytes(login));
            webRequest.Headers.Add(HttpRequestHeader.Authorization, $"Basic {authorization}");

            //post request
            var postData = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(request));
            webRequest.ContentLength = postData.Length;
            using (var stream = webRequest.GetRequestStream())
                stream.Write(postData, 0, postData.Length);

            //get response
            var httpResponse = (HttpWebResponse)webRequest.GetResponse();
            var responseMessage = string.Empty;
            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                responseMessage = streamReader.ReadToEnd();

            var response = JsonConvert.DeserializeObject<TResponse>(responseMessage)
                ?? throw new NopException("An error occurred while processing. Error details in the log.");

            //whether the request is succeeded
            if (response.ResponseCode != ResponseCode.OK)
                throw new NopException($"{response.ResponseCode}. {response.Message}");

            return response;
        }

        /// <summary>
        /// Handle request action
        /// </summary>
        /// <typeparam name="T">Response type</typeparam>
        /// <param name="requestAction">Request action</param>
        /// <returns>Response</returns>
        private T HandleRequestAction<T>(Func<T> requestAction)
        {
            try
            {
                //process request action
                return requestAction();
            }
            catch (Exception exception)
            {
                var errorMessage = $"Qualpay Checkout error: {exception.Message}.";
                try
                {
                    //try to get error response
                    if (exception is WebException webException)
                    {
                        var httpResponse = (HttpWebResponse)webException.Response;
                        using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                        {
                            var errorResponse = streamReader.ReadToEnd();
                            errorMessage = $"{errorMessage} Details: {errorResponse}";
                            return JsonConvert.DeserializeObject<T>(errorResponse);
                        }
                    }
                }
                catch { }
                finally
                {
                    //log errors
                    _logger.Error(errorMessage, exception, _workContext.CurrentCustomer);
                }

                return default(T);
            }
        }

        /// <summary>
        /// Send email about new subscription/unsubscription
        /// </summary>
        /// <param name="email">From email address</param>
        /// <param name="subscribe">Whether to subscribe the specified email</param>
        private void SendEmail(string email, bool subscribe)
        {
            //try to get an email account
            var emailAccount = _emailAccountService.GetEmailAccountById(_emailAccountSettings.DefaultEmailAccountId)
                ?? throw new NopException("Email account could not be loaded");

            var subject = subscribe ? "New subscription" : "New unsubscription";
            var body = subscribe
                ? "nopCommerce user just left the email to receive an information about special offers from Qualpay."
                : "nopCommerce user has canceled subscription to receive Qualpay news.";

            //send email
            _emailSender.SendEmail(emailAccount: emailAccount,
                subject: subject, body: body,
                fromAddress: email, fromName: QualpayCheckoutDefaults.UserAgent,
                toAddress: QualpayCheckoutDefaults.SubscriptionEmail, toName: null);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Post request to Qualpay Checkout
        /// </summary>
        /// <param name="request">Request</param>
        /// <returns>Response details</returns>
        public QualpayCheckoutResponseDetails Checkout(QualpayCheckoutRequest request)
        {
            return HandleRequestAction(() =>
            {
                return PostRequest<QualpayCheckoutRequest, QualpayCheckoutResponse>(request)?.Details
                    ?? throw new NopException("An error occurred while processing. Error details in the log.");
            });
        }

        /// <summary>
        /// Get transaction details from IPN
        /// </summary>
        /// <returns>Transaction object and transaction raw string</returns>
        public (Transaction, string) GetTransaction()
        {
            //get transaction from request
            return HandleRequestAction(() =>
            {
                using (var streamReader = new StreamReader(_httpContextAccessor.HttpContext.Request.Body))
                {
                    var transactionString = streamReader.ReadToEnd();
                    var transaction = JsonConvert.DeserializeObject<Transaction>(transactionString);

                    return (transaction, transactionString);
                }
            });
        }

        /// <summary>
        /// Subscribe to Qualpay news
        /// </summary>
        /// <param name="email">Email address</param>
        /// <returns>True if successfully subscribed/unsubscribed, otherwise false</returns>
        public bool SubscribeToQualpay(string email)
        {
            return HandleRequestAction(() =>
            {
                //unsubscribe previous email
                if (!string.IsNullOrEmpty(_qualpayCheckoutSettings.MerchantEmail))
                    SendEmail(_qualpayCheckoutSettings.MerchantEmail, false);

                //subscribe new email
                if (!string.IsNullOrEmpty(email))
                    SendEmail(email, true);

                return true;
            });
        }

        #endregion
    }
}