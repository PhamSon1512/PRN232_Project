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
        private readonly IConfiguration _configuration;

        public PaymentsController(IVnPayService vnPayService, IWalletService walletService, IConfiguration configuration)
        {
            _vnPayService = vnPayService;
            _walletService = walletService;
            _configuration = configuration;
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

            //// Lấy userId từ claim và parse về Guid
            //var userIdString = User.FindFirst(c => c.Type == "UserId")?.Value;
            //if (!string.IsNullOrEmpty(userIdString) && Guid.TryParse(userIdString, out var userId))
            //{
            //    // Kiểm tra response thành công và có OrderDescription
            //    if (response.Success && !string.IsNullOrEmpty(response.OrderDescription))
            //    {
            //        var parts = response.OrderDescription.Split('|');

            //        // Parse phần tử thứ 2 làm amount
            //        if (parts.Length >= 2 && decimal.TryParse(parts[1], out var amount))
            //        {
            //            try
            //            {
            //                await _walletService.DepositAsync(userId, amount, response.TransactionId);
            //            }
            //            catch (Exception ex)
            //            {
            //                // Log lỗi nhưng vẫn redirect để FE xử lý
            //                Console.WriteLine($"Lỗi khi cập nhật số dư: {ex.Message}");
            //            }
            //        }
            //    }
            //}

            // Lấy URL từ cấu hình và thêm query string
            var returnUrl = _configuration["PaymentCallBack:ReturnUrl"];
            var queryString = Request.QueryString.ToString();
            return Redirect($"{returnUrl}{queryString}");
        }

    }
}