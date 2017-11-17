using Microsoft.AspNetCore.Mvc;
using Nop.Web.Framework.Components;

namespace Nop.Plugin.Payments.QualpayCheckout.Components
{
    [ViewComponent(Name = "QualpayCheckout")]
    public class QualpayCheckoutViewComponent : NopViewComponent
    {
        public IViewComponentResult Invoke()
        {
            return View("~/Plugins/Payments.QualpayCheckout/Views/PaymentInfo.cshtml");
        }
    }
}
