using System.Web.Mvc;
using System.Web.Routing;
using Nop.Web.Framework.Mvc.Routes;

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
        /// <param name="routes">Current route collection</param>
        public void RegisterRoutes(RouteCollection routes)
        {
            //IPN
            routes.MapRoute("Plugin.Payments.QualpayCheckout.IPN",
                 "Plugins/QualpayCheckout/IPN",
                 new { controller = "QualpayCheckout", action = "IPNHandler" },
                 new[] { "Nop.Plugin.Payments.QualpayCheckout.Controllers" }
            );
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
