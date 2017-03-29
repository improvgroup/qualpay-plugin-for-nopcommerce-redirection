using System;
using System.IO;
using System.Net;
using System.Text;
using System.Web;
using Newtonsoft.Json;
using Nop.Plugin.Payments.QualpayCheckout.Domain;
using Nop.Services.Logging;

namespace Nop.Plugin.Payments.QualpayCheckout.Helpers
{
    /// <summary>
    /// Represents Qualpay Checkout helper
    /// </summary>
    public static class QualpayCheckoutHelper
    {
        #region Utilities

        /// <summary>
        /// Gets Qualpay Checkout service URL
        /// </summary>
        /// <param name="qualpayCheckoutSettings">Qualpay Checkout settings</param>
        /// <returns>URL</returns>
        private static string GetQualpayCheckoutServiceUrl(QualpayCheckoutSettings qualpayCheckoutSettings)
        {
            return qualpayCheckoutSettings.UseSandbox
                ? "https://app-test.qualpay.com/service/api/"
                : "https://app.qualpay.com/service/api/";
        }

        #endregion

        #region Methods

        /// <summary>
        /// Post request and get response to Qualpay Checkout service
        /// </summary>
        /// <param name="checkoutRequest">Request details</param>
        /// <param name="qualpayCheckoutSettings">Qualpay Checkout settings</param>
        /// <param name="logger">Logger</param>
        /// <returns>Response from Qualpay Checkout service</returns>
        public static QualpayCheckoutResponse PostCheckoutRequest(QualpayCheckoutRequest checkoutRequest, 
            QualpayCheckoutSettings qualpayCheckoutSettings, ILogger logger)
        {
            //create request
            var url = string.Format("{0}checkout", GetQualpayCheckoutServiceUrl(qualpayCheckoutSettings));
            var request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "POST";
            request.ContentType = "application/json; charset=utf-8";
            request.Accept = "application/json";

            //set authentication
            var login = string.Format("{0}:{1}", qualpayCheckoutSettings.MerchantId, qualpayCheckoutSettings.SecurityKey);
            var authorization = Convert.ToBase64String(Encoding.UTF8.GetBytes(login));
            request.Headers.Add(HttpRequestHeader.Authorization, string.Format("Basic {0}", authorization));

            //set post data
            var postData = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(checkoutRequest));
            request.ContentLength = postData.Length;

            //post request
            try
            {
                using (var stream = request.GetRequestStream())
                {
                    stream.Write(postData, 0, postData.Length);
                }

                //get response
                var httpResponse = (HttpWebResponse)request.GetResponse();
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    return JsonConvert.DeserializeObject<QualpayCheckoutResponse>(streamReader.ReadToEnd());
                }
            }
            catch (WebException ex)
            {
                try
                {
                    var httpResponse = (HttpWebResponse)ex.Response;
                    using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                    {
                        //log errors
                        var response = streamReader.ReadToEnd();
                        logger.Error(string.Format("Qualpay Checkout error: {0}", response), ex);

                        return JsonConvert.DeserializeObject<QualpayCheckoutResponse>(response);
                    }
                }
                catch (Exception exc)
                {
                    logger.Error("Qualpay Checkout error", exc);
                    return null;
                }
            }
            catch (Exception exc)
            {
                logger.Error("Qualpay Checkout error", exc);
                return null;
            }
        }

        /// <summary>
        /// Get transaction details from notification
        /// </summary>
        /// <param name="httpContext">HTTP context</param>
        /// <param name="logger">Logger</param>
        /// <param name="transactionString">String representation of transaction</param>
        /// <returns>Transaction</returns>
        public static Transaction GetTransaction(HttpContextBase httpContext, ILogger logger, out string transactionString)
        {
            //get transaction from request
            try
            {
                using (var streamReader = new StreamReader(httpContext.Request.InputStream))
                {
                    transactionString = streamReader.ReadToEnd();
                    return JsonConvert.DeserializeObject<Transaction>(transactionString);
                }
            }
            catch (Exception ex)
            {
                //log errors
                logger.Error("Qualpay Checkout IPN error", ex);
                transactionString = string.Empty;
                return null;
            }
        }

        #endregion
    }
}
