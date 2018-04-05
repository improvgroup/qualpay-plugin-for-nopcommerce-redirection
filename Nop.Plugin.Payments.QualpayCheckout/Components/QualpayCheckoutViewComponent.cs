using Microsoft.AspNetCore.Mvc;
using Nop.Web.Framework.Components;

namespace Nop.Plugin.Payments.QualpayCheckout.Components
{
    /// <summary>
    /// Represents payment info view component
    /// </summary>
    [ViewComponent(Name = QualpayCheckoutDefaults.ViewComponentName)]
    public class QualpayCheckoutViewComponent : NopViewComponent
    {
        #region Methods

        /// <summary>
        /// Invoke view component
        /// </summary>
        /// <returns>View component result</returns>
        public IViewComponentResult Invoke()
        {
            return View("~/Plugins/Payments.QualpayCheckout/Views/PaymentInfo.cshtml");
        }

        #endregion
    }
}