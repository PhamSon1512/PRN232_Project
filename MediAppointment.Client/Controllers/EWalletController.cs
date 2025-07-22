using MediAppointment.Client.Attributes;
using MediAppointment.Client.Models.EWallet;
using MediAppointment.Client.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace MediAppointment.Client.Controllers
{
    [RequirePatient]
    [Route("Ewallet")]
    public class EWalletController : Controller
    {
        private readonly IWalletApiService _walletApiService;
        private readonly ILogger<EWalletController> _logger;

        public EWalletController(IWalletApiService walletApiService, ILogger<EWalletController> logger)
        {
            _walletApiService = walletApiService;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var model = new WalletViewModel
                {
                    Balance = 0,
                    Transactions = new List<WalletTransaction>(),
                    IsConnected = false,
                    ErrorMessage = null
                };

                try
                {
                    var balance = await _walletApiService.GetBalanceAsync();
                    var transactions = await _walletApiService.GetTransactionsAsync();

                    model.Balance = balance;
                    model.Transactions = transactions;
                    model.IsConnected = true;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to load wallet data");
                    model.ErrorMessage = ex.Message;
                }

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in wallet index");
                var errorModel = new WalletViewModel
                {
                    Balance = 0,
                    Transactions = new List<WalletTransaction>(),
                    IsConnected = false,
                    ErrorMessage = "Có lỗi xảy ra khi tải dữ liệu ví điện tử."
                };
                return View(errorModel);
            }
        }

        [HttpGet("get-balance")]
        public async Task<IActionResult> GetBalance()
        {
            try
            {
                var balance = await _walletApiService.GetBalanceAsync();
                return Json(new { success = true, balance });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get balance");
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet("get-transactions")]
        public async Task<IActionResult> GetTransactions()
        {
            try
            {
                var transactions = await _walletApiService.GetTransactionsAsync();
                return Json(new { success = true, transactions });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get transactions");
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost("deposit-url")]
        public async Task<IActionResult> CreatePaymentUrl([FromBody] DepositRequest model)
        {
            if (model == null || model.Amount < 1000)
            {
                return Json(new { success = false, message = "Số tiền nạp tối thiểu là 1,000 VND" });
            }

            if (model.Amount > 50000000) // 50 triệu VND
            {
                return Json(new { success = false, message = "Số tiền nạp tối đa là 50,000,000 VND" });
            }

            try
            {
                var paymentUrl = await _walletApiService.CreatePaymentUrlAsync(model.Amount);

                if (string.IsNullOrEmpty(paymentUrl))
                {
                    return Json(new { success = false, message = "Không thể tạo URL thanh toán" });
                }

                return Json(new { success = true, paymentUrl });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create deposit with amount {Amount}", model.Amount);
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost("deposit")]
        public async Task<IActionResult> Deposit([FromBody] DepositToWalletRequest request)
        {
            if (request == null || request.UserId == Guid.Empty || request.Amount <= 0)
                return Json(new { success = false, message = "Dữ liệu không hợp lệ." });

            try
            {
                var result = await _walletApiService.DepositAsync((Guid)(request?.UserId), request.Amount, request.TransactionId ?? "");
                return Json(new { success = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xác nhận nạp tiền cho user {UserId}", request.UserId);
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh()
        {
            try
            {
                var balance = await _walletApiService.GetBalanceAsync();
                var transactions = await _walletApiService.GetTransactionsAsync();

                return Json(new
                {
                    success = true,
                    balance,
                    transactions,
                    message = "Dữ liệu đã được cập nhật"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to refresh wallet data");
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        [Route("check-balance-payment")]
        public async Task<IActionResult> CheckAndProcessPayment([FromBody] DepositRequest model)
        {
            if (model.Amount <= 0)
            {
                return Json(new { success = false, message = "Yêu cầu không hợp lệ" });
            }
            var balance = await _walletApiService.GetBalanceAsync();
            if (balance < model.Amount)
                return Json(new { success = false, message = "Số dư không đủ để thực hiện giao dịch, vui lòng nạp thêm tiền vào ví và thực hiện lại" });

            try
            {
                
                return Json(new { success = await _walletApiService.ReductAsync(model.Amount), message = "Đặt lịch thành công" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Lỗi hệ thống: {ex.Message}" });
            }
        }

        public class DepositRequest
        {
            public decimal Amount { get; set; }
        }

        public class DepositToWalletRequest
        {
            public Guid? UserId { get; set; }
            public decimal Amount { get; set; }
            public string? TransactionId { get; set; }
        }
    }
}