using MediAppointment.Application.DTOs;
using MediAppointment.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace MediAppointment.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DoctorController : ControllerBase
    {
        private readonly IProfileService _profileService;

        public DoctorController(IProfileService profileService)
        {
            _profileService = profileService;
        }

        [HttpGet("profile/{userIdentityId:guid}")]
        public async Task<IActionResult> Profile(Guid userIdentityId)
        {
            var doctor = await _profileService.GetProfileByIdAsync(userIdentityId);
            if (doctor == null) return NotFound();

            return Ok(doctor);
        }

        [HttpPut("profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] DoctorUpdateDto dto)
        {
            try
            {
                var updatedDoctor = await _profileService.UpdateProfileAsync(dto);
                return Ok(updatedDoctor);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}