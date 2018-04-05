using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Nop.Web.Framework.Mvc.Routing;

namespace Nop.Plugin.Payments.QualpayCheckout.Infrastructure
{
    /// <summary>
    /// Represents plugin route provider
    /// </summary>
    public partial class RouteProvider : IRouteProvider
    {
        /// <summary>
        /// Register routes
        /// </summary>
        /// <param name="routeBuilder">Route builder</param>
        public void RegisterRoutes(IRouteBuilder routeBuilder)
        {
            //add route to the IPN handler
            routeBuilder.MapRoute(QualpayCheckoutDefaults.IpnRouteName, "Plugins/QualpayCheckout/IPN/",
                new { controller = "Ipn", action = "IpnHandler" });
        }

        /// <summary>
        /// Gets a priority of route provider
        /// </summary>
        public int Priority
        {
            get { return 0; }
        }
    }
}