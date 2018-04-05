using System;
using Microsoft.AspNetCore.Mvc;
using Nop.Core.Domain.Orders;
using Nop.Plugin.Payments.QualpayCheckout.Services;
using Nop.Services.Common;
using Nop.Services.Orders;
using Nop.Web.Framework.Controllers;

namespace Nop.Plugin.Payments.QualpayCheckout.Controllers
{
    public class IpnController : BaseController
    {
        #region Fields

        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IOrderProcessingService _orderProcessingService;
        private readonly IOrderService _orderService;
        private readonly QualpayCheckoutManager _qualpayCheckoutManager;

        #endregion

        #region Ctor

        public IpnController(IGenericAttributeService genericAttributeService,
            IOrderProcessingService orderProcessingService,
            IOrderService orderService,
            QualpayCheckoutManager qualpayCheckoutManager)
        {
            this._genericAttributeService = genericAttributeService;
            this._orderProcessingService = orderProcessingService;
            this._orderService = orderService;
            this._qualpayCheckoutManager = qualpayCheckoutManager;
        }

        #endregion

        #region Methods

        [HttpPost]
        public IActionResult IpnHandler()
        {
            //get Qualpay transaction 
            var (transaction, transactionString) = _qualpayCheckoutManager.GetTransaction();
            if (transaction == null)
                return BadRequest();

            //try to get an order for this transaction
            var order = _orderService.GetOrderByCustomOrderNumber(transaction.PurchaseId);
            if (order == null)
                return Ok();

            //validate received transaction by comparing some of data
            var checkoutId = order.GetAttribute<string>(QualpayCheckoutDefaults.CheckoutIdAttribute) ?? string.Empty;
            if (!checkoutId.Equals(transaction.CheckoutId))
                return Ok();

            //add order note
            order.OrderNotes.Add(new OrderNote()
            {
                Note = transactionString,
                DisplayToCustomer = false,
                CreatedOnUtc = DateTime.UtcNow
            });
            _orderService.UpdateOrder(order);

            //compare amounts
            if (Math.Round(order.OrderTotal, 2) != Math.Round(transaction.Amount, 2))
                return Ok();

            //all is ok, so paid order
            if (_orderProcessingService.CanMarkOrderAsPaid(order))
            {
                //set payment details
                order.AuthorizationTransactionCode = transaction.AuthorizationCode;
                order.CaptureTransactionId = transaction.TransactionId;
                order.CaptureTransactionResult = transaction.ResponseMessage;
                _orderService.UpdateOrder(order);

                _orderProcessingService.MarkOrderAsPaid(order);
            }

            //delete validation data
            _genericAttributeService.SaveAttribute<string>(order, QualpayCheckoutDefaults.CheckoutIdAttribute, null);

            return Ok();
        }

        #endregion
    }
}