using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Nop.Web.Framework.Mvc.Routing;
using Nop.Web.Infrastructure;

namespace Nop.Plugin.Payments.AmazonPay.Infrastructure
{
    /// <summary>
    /// Represents plugin route provider
    /// </summary>
    public class RouteProvider : BaseRouteProvider, IRouteProvider
    {
        /// <summary>
        /// Register routes
        /// </summary>
        /// <param name="endpointRouteBuilder">Route builder</param>
        public void RegisterRoutes(IEndpointRouteBuilder endpointRouteBuilder)
        {
            //get language pattern
            //it's not needed to use language pattern in AJAX requests and for actions returning the result directly (e.g. file to download),
            //use it only for URLs of pages that the user can go to
            var lang = GetLanguageRoutePattern();

            //confirm order
            endpointRouteBuilder.MapControllerRoute(AmazonPayDefaults.ConfirmRouteName,
                $"{lang}/amazon-pay/confirm",
                new { controller = "AmazonPayCheckout", action = "Confirm" });

            //completed page
            endpointRouteBuilder.MapControllerRoute(AmazonPayDefaults.CheckoutResultHandlerRouteName,
                $"{lang}/amazon-pay/completed",
                new { controller = "AmazonPayCheckout", action = "Completed" });

            //set sign in method
            endpointRouteBuilder.MapControllerRoute(AmazonPayDefaults.SignInHandlerRouteName,
                $"{lang}/amazon-pay/sign-in",
                new { controller = "AmazonPayCustomer", action = "SignIn" });

            //IPN
            endpointRouteBuilder.MapControllerRoute(AmazonPayDefaults.IPNHandlerRouteName,
                "amazon-pay/ipn",
                new { controller = "AmazonPayIpn", action = "IPNHandler" });

            //onboarding
            endpointRouteBuilder.MapControllerRoute(AmazonPayDefaults.Onboarding.KeyShareRouteName,
                "amazon-pay/key-share",
                new { controller = "AmazonPayOnboarding", action = "KeyShare" });
        }

        /// <summary>
        /// Gets a priority of route provider
        /// </summary>
        public int Priority => 0;
    }
}