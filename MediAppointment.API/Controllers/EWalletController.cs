using Azure.Core;
using MediAppointment.Application.DTOs.Payments;
using MediAppointment.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace MediAppointment.API.Controllers
{
    [Authorize(Roles = "Patient")]
    [ApiController]
    [Route("api/wallet")]
    public class EWalletController : ControllerBase
    {
        private readonly IWalletService _walletService;
        private readonly IHttpContextAccessor _httpContextAccessor;


        public EWalletController(IWalletService walletService, IHttpContextAccessor httpContextAccessor)
        {
            _walletService = walletService;
            _httpContextAccessor = httpContextAccessor;
        }

        [HttpGet("balance")]
        public async Task<IActionResult> GetBalance()
        {
            //var userIdString = User.FindFirst(c => c.Type == "UserId")?.Value;
            var userIdString = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!Guid.TryParse(userIdString, out var userId))
            {
                return Unauthorized(new { message = "UserId không hợp lệ" });
            }

            var balance = await _walletService.GetBalanceAsync(userId);
            return Ok(new { balance });
        }

        [HttpGet("transactions")]
        public async Task<IActionResult> GetTransactions()
        {
            //var userIdString = User.FindFirst(c => c.Type == "UserId")?.Value;
            var userIdString = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!Guid.TryParse(userIdString, out var userId))
            {
                return Unauthorized(new { message = "UserId không hợp lệ" });
            }

            var transactions = await _walletService.GetTransactionsAsync(userId);
            return Ok(transactions);
        }

        [HttpPost("deposit")]
        public async Task<IActionResult> Deposit([FromBody] DepositRequest request)
        {
            //var userIdString = User.FindFirst(c => c.Type == "UserId")?.Value;
            var userIdString = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!Guid.TryParse(userIdString, out var userId))
            {
                return Unauthorized(new { message = "UserId không hợp lệ" });
            }

            if (request == null || userId == Guid.Empty || request.Amount <= 0)
            {
                return BadRequest("Thông tin yêu cầu không hợp lệ.");
            }

            try
            {
                await _walletService.DepositAsync(userId, request.Amount, request.TransactionId ?? "");
                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpPost("reduct")]
        public async Task<IActionResult> Reduct([FromBody] ReductRequest model)
        {
            var userIdString = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!Guid.TryParse(userIdString, out var userId))
            {
                return Unauthorized(new { success = false, message = "UserId không hợp lệ" });
            }

            try
            {
                await _walletService.ReductAsync(userId, model.Amount);
                return Ok(new { success = true, message = "Thanh toán thành công" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

    }

    public class ReductRequest
    {
        public decimal Amount { get; set; }
    }

}