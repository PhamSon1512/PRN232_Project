using MediAppointment.Application.DTOs;
using MediAppointment.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace MediAppointment.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IIdentityService _identityService;

        public AuthController(IIdentityService identityService)
        {
            _identityService = identityService;
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
            if (dto == null || dto.Roles == null || !dto.Roles.Any())
                return BadRequest("Invalid registration data or roles are required.");

            var result = await _identityService.RegisterAsync(dto);
            if (!result.Success)
                return BadRequest(result.ErrorMessage);

            return Ok(result);
        }
    }
}
