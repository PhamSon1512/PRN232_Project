using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MediAppointment.Infrastructure.Data;

namespace MediAppointment.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DepartmentController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<DepartmentController> _logger;

        public DepartmentController(ApplicationDbContext context, ILogger<DepartmentController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetDepartments()
        {
            try
            {
                var departments = await _context.Departments
                    .Select(d => new
                    {
                        Id = d.Id,
                        Name = d.DepartmentName
                    })
                    .ToListAsync();

                return Ok(departments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting departments");
                return StatusCode(500, new { message = "Có lỗi xảy ra khi lấy danh sách khoa" });
            }
        }

        [HttpGet("GetRoomsByDepartment/{departmentId}")]
        public async Task<IActionResult> GetRoomsByDepartment(Guid departmentId)
        {
            var rooms = await _context.Room
                .Where(r => r.DepartmentId == departmentId)
                .Select(r => new { r.Id, r.Name, r.DepartmentId })
                .ToListAsync();
            return Ok(rooms);
        }

        [HttpGet("test")]
        public IActionResult TestConnection()
        {
            return Ok(new { message = "Department API is working", timestamp = DateTime.Now });
        }
    }
}
