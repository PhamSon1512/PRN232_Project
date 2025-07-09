using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MediAppointment.Infrastructure.Data;

namespace MediAppointment.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RoomController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<RoomController> _logger;

        public RoomController(ApplicationDbContext context, ILogger<RoomController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet("department/{departmentId}")]
        public async Task<IActionResult> GetRoomsByDepartment(Guid departmentId)
        {
            try
            {
                if (departmentId == Guid.Empty)
                {
                    return BadRequest(new { message = "Department ID không hợp lệ" });
                }

                var rooms = await _context.Room
                    .Where(r => r.DepartmentId == departmentId)
                    .Select(r => new
                    {
                        Id = r.Id,
                        Name = r.Name,
                        DepartmentId = r.DepartmentId
                    })
                    .ToListAsync();

                return Ok(rooms);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting rooms for department {DepartmentId}", departmentId);
                return StatusCode(500, new { message = "Có lỗi xảy ra khi lấy danh sách phòng" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAllRooms()
        {
            try
            {
                var rooms = await _context.Room
                    .Include(r => r.Department)
                    .Select(r => new
                    {
                        Id = r.Id,
                        Name = r.Name,
                        DepartmentId = r.DepartmentId,
                        DepartmentName = r.Department.DepartmentName
                    })
                    .ToListAsync();

                return Ok(rooms);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all rooms");
                return StatusCode(500, new { message = "Có lỗi xảy ra khi lấy danh sách phòng" });
            }
        }

        [HttpGet("test")]
        public IActionResult TestConnection()
        {
            return Ok(new { message = "Room API is working", timestamp = DateTime.Now });
        }
    }
}
