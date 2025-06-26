using System.Security.Claims;
using MediAppointment.Application.DTOs;
using MediAppointment.Application.DTOs.Auth;
using MediAppointment.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MediAppointment.API.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IIdentityService _identityService;
        private readonly ITokenService _tokenService;

        public AuthController(IIdentityService identityService, ITokenService tokenService)
        {
            _identityService = identityService;
            _tokenService = tokenService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            var result = await _identityService.LoginAsync(dto);
            if (!result.Success)
                return Unauthorized(result.ErrorMessage);

            return Ok(result);
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            var result = await _identityService.RegisterAsync(dto);
            if (!result.Success)
                return BadRequest(result.ErrorMessage);

            return Ok(result);
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> Refresh(RefreshTokenDto dto)
        {
            var result = await _identityService.RefreshTokenAsync(dto);
            return Ok(result);
        }

        [AllowAnonymous]
        [HttpGet("confirm-email")]
        public async Task<IActionResult> ConfirmEmail([FromQuery] string email, [FromQuery] string token)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(token))
                return BadRequest("Thông tin xác minh không hợp lệ.");

            var result = await _identityService.ConfirmEmailAsync(email, token);

            if (!result.Success)
                return BadRequest(result.ErrorMessage);

            return Ok("✅ Xác minh email thành công. Bạn có thể đăng nhập.");
        }


        [AllowAnonymous]
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.Email))
                return BadRequest("Email is required.");

            var result = await _identityService.ForgotPasswordAsync(dto);
            if (!result)
                return BadRequest("Không thể gửi email đặt lại mật khẩu. Vui lòng kiểm tra lại email.");

            return Ok("Đã gửi email đặt lại mật khẩu (nếu email tồn tại trong hệ thống).");
        }

        [AllowAnonymous]
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword(ResetPasswordDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Token) || string.IsNullOrWhiteSpace(dto.NewPassword))
                return BadRequest("Thông tin không hợp lệ.");

            var result = await _identityService.ResetPasswordAsync(dto);
            if (!result.Success)
                return BadRequest(result.ErrorMessage);

            return Ok(result);
        }


        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            await _identityService.LogoutAsync();
            return Ok("Đăng xuất thành công.");
        }


    }
}