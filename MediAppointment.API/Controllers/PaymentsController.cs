using MediAppointment.Application.DTOs.Payments;
using MediAppointment.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace MediAppointment.API.Controllers
{
    [ApiController]
    [Route("api/payments")]
    public class PaymentsController : ControllerBase
    {
        private readonly IVnPayService _vnPayService;
        private readonly IWalletService _walletService;

        public PaymentsController(IVnPayService vnPayService, IWalletService walletService)
        {
            _vnPayService = vnPayService;
            _walletService = walletService;
        }

        [HttpPost("create-payment-url")]
        public IActionResult CreatePaymentUrl([FromBody] PaymentInformationModel model)
        {
            var url = _vnPayService.CreatePaymentUrl(model, HttpContext);
            return Ok(new { paymentUrl = url });
        }

        [HttpGet("payment-callback")]
        public async Task<IActionResult> PaymentCallback()
        {
            var response = _vnPayService.PaymentExecute(Request.Query);

            if (response.Success)
            {
                Guid userId;
                decimal amount = 0;

                if (Guid.TryParse(response.OrderId, out userId))
                {
                    amount = 1000000; 
                    await _walletService.DepositAsync(userId, amount, response.TransactionId);
                }
            }

            return Ok(response);
        }


    }
}
