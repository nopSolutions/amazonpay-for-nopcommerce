using Microsoft.AspNetCore.Mvc;
using Nop.Core.Http;
using Nop.Plugin.Payments.AmazonPay.Enums;
using Nop.Plugin.Payments.AmazonPay.Services;
using Nop.Services.Messages;
using Nop.Web.Controllers;

namespace Nop.Plugin.Payments.AmazonPay.Controllers;

[AutoValidateAntiforgeryToken]
public class AmazonPayCheckoutController : BasePublicController
{
    #region Fields

    private readonly AmazonPayCheckoutService _amazonPayCheckoutService;
    private readonly INotificationService _notificationService;

    #endregion

    #region Ctor

    public AmazonPayCheckoutController(AmazonPayCheckoutService amazonPayCheckoutService,
        INotificationService notificationService)
    {
        _amazonPayCheckoutService = amazonPayCheckoutService;
        _notificationService = notificationService;
    }

    #endregion

    #region Methods

    public async Task<IActionResult> Confirm()
    {
        try
        {
            await _amazonPayCheckoutService.ReadAndSaveCheckoutSessionIdAsync();

            //validation
            var cart = await _amazonPayCheckoutService.GetCartAsync();
            if (!cart.Any())
                return RedirectToRoute(NopRouteNames.General.CART);

            if (!await _amazonPayCheckoutService.IsAllowedToCheckoutAsync())
                return Challenge();

            //model
            var model = await _amazonPayCheckoutService.PrepareConfirmOrderModelAsync(cart);

            return View("~/Plugins/Payments.AmazonPay/Views/Confirm.cshtml", model);
        }
        catch (Exception exception)
        {
            _notificationService.ErrorNotification(exception.Message);

            return RedirectToRoute(NopRouteNames.General.CART);
        }
    }

    [HttpPost, ActionName("Confirm")]
    public async Task<IActionResult> ConfirmOrder(string shippingoption)
    {
        try
        {
            //validation
            var cart = await _amazonPayCheckoutService.GetCartAsync();
            if (!cart.Any())
                return RedirectToRoute(NopRouteNames.General.CART);

            if (!await _amazonPayCheckoutService.IsAllowedToCheckoutAsync())
                return Challenge();

            if (!await _amazonPayCheckoutService.ShoppingCartRequiresShippingAsync(cart))
                await _amazonPayCheckoutService.SetShippingMethodToNullAsync();
            else
            {
                var success = await _amazonPayCheckoutService.SetShippingMethodAsync(cart, shippingoption);

                if (!success)
                    return RedirectToRoute(AmazonPayDefaults.ConfirmRouteName);
            }

            var scWarnings = await _amazonPayCheckoutService.GetShoppingCartWarningsAsync(cart);
            if (!string.IsNullOrEmpty(scWarnings))
            {
                _notificationService.WarningNotification(string.Join("<br />", scWarnings));

                return RedirectToRoute(NopRouteNames.General.CART);
            }

            var url = await _amazonPayCheckoutService.UpdateCheckoutSessionAsync();

            return Redirect(url);
        }
        catch (Exception exception)
        {
            _notificationService.ErrorNotification(exception.Message);

            return RedirectToRoute(NopRouteNames.General.CART);
        }
    }

    [HttpPost]
    public async Task<IActionResult> GetPaymentInfo(int placement)
    {
        var (payload, signature, amount) = await _amazonPayCheckoutService.CreateCheckoutSessionAsync((ButtonPlacement)placement);

        if (string.IsNullOrEmpty(payload))
            return ErrorJson($"{AmazonPayDefaults.PluginSystemName} error: Payload creation failed");

        if (string.IsNullOrEmpty(signature))
            return ErrorJson($"{AmazonPayDefaults.PluginSystemName} error: Signature validation failed");

        return Json(new { payload, signature, amount });
    }

    public async Task<IActionResult> Completed()
    {
        try
        {
            var cart = await _amazonPayCheckoutService.GetCartAsync();
            if (!cart.Any())
                return RedirectToRoute(NopRouteNames.General.CART);

            var (order, warnings, model) = await _amazonPayCheckoutService.CompleteCheckoutAsync();

            if (order == null)
            {
                if (warnings.Any())
                    _notificationService.WarningNotification(string.Join("<br />", warnings));

                return RedirectToRoute(NopRouteNames.General.CART);
            }

            return View("~/Plugins/Payments.AmazonPay/Views/Completed.cshtml", model);
        }
        catch (Exception exception)
        {
            _notificationService.ErrorNotification(exception.Message);

            return RedirectToRoute(NopRouteNames.General.CART);
        }
    }

    #endregion
}