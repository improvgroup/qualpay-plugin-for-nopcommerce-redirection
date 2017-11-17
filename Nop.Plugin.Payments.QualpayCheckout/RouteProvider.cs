using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Nop.Web.Framework.Mvc.Routing;

namespace Nop.Plugin.Payments.QualpayCheckout
{
    /// <summary>
    /// Represents custom route provider
    /// </summary>
    public partial class RouteProvider : IRouteProvider
    {
        /// <summary>
        /// Register routes
        /// </summary>
        /// <param name="routeBuilder">Route builder</param>
        public void RegisterRoutes(IRouteBuilder routeBuilder)
        {
            //IPN
            routeBuilder.MapRoute("Plugin.Payments.QualpayCheckout.IPN",
                 "Plugins/QualpayCheckout/IPN",
                 new { controller = "QualpayCheckout", action = "IPNHandler" });
        }

        /// <summary>
        /// Gets or sets the priority
        /// </summary>
        public int Priority
        {
            get { return 0; }
        }
    }
}
