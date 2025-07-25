using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;
using MediAppointment.Application.DTOs.GeminiDTOs;
using MediAppointment.Application.Interfaces;
using MediAppointment.Domain.Entities;
using MediAppointment.Domain.Entities.Abstractions;
using MediAppointment.Domain.Enums;
using MediAppointment.Infrastructure.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace MediAppointment.Infrastructure.Services
{
    public class GeminiChatService : IGeminiChatService
    {
        private readonly HttpClient _httpClient;
        private readonly GeminiSettings _settings;
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public GeminiChatService(HttpClient httpClient, IOptions<GeminiSettings> options, ApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _httpClient = httpClient;
            _settings = options.Value;
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<string> SendMessageAsync(string message, Guid? userId = null, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                return "Vui lòng nhập tin nhắn.";
            }

            message = message.ToLower().Trim();

            // Nếu userId không được truyền vào, tự động lấy từ JWT token
            if (!userId.HasValue)
            {
                userId = ExtractUserIdFromContext();
            }

            Guid? actualUserId = userId.HasValue ? await GetUserIdFromIdentityId(userId.Value, cancellationToken) : null;

            if (Regex.IsMatch(message, @"(hi|hello|chào|how are you|how's it going|how are you today)"))
            {
                return "Chào bạn! Tôi là chatbot MediAppointment, tôi đang hoạt động tốt. Bạn khỏe không? Hôm nay tôi có thể giúp gì cho bạn?";
            }

            // 1. Lịch sử giao dịch
            if (Regex.IsMatch(message, @"(lịch sử giao dịch|giao dịch gần đây|transaction history)"))
            {
                if (!actualUserId.HasValue)
                    return "Vui lòng đăng nhập để xem lịch sử giao dịch.";
                return await GetTransactionHistoryAsync(actualUserId.Value, cancellationToken);
            }
            // 2. Khung giờ trống
            else if (Regex.IsMatch(message, @"(khung giờ trống|slot khám bệnh|available slots)"))
            {
                var date = ParseDateFromMessage(message);
                var doctorName = ExtractDoctorName(message);
                return await GetAvailableTimeSlotsAsync(date, doctorName, cancellationToken);
            }
            // 3. Danh sách bác sĩ
            else if (Regex.IsMatch(message, @"(danh sách bác sĩ|liệt kê bác sĩ|tất cả bác sĩ|bác sĩ trong hệ thống|all doctors|những bác sĩ)"))
            {
                return await GetAllDoctorsAsync(cancellationToken);
            }
            // 4. Danh sách khoa
            else if (Regex.IsMatch(message, @"(danh sách khoa|những khoa|các khoa trong hệ thống|list departments|departments available|khoa nào có sẵn)"))
            {
                return await GetAllDepartmentsAsync(cancellationToken);
            }
            // 5. Bác sĩ theo ngày
            else if (Regex.IsMatch(message, @"(bác sĩ.*?làm việc|doctors.*?working|bác sĩ.*?ngày|doctors.*?on|làm việc.*?bác sĩ|working.*?doctors|có.*?bác sĩ.*?làm|bác sĩ.*?hôm nay|bác sĩ.*?ngày mai|bác sĩ.*?thứ|doctors.*?today|doctors.*?tomorrow|doctors.*?monday|doctors.*?tuesday|doctors.*?wednesday|doctors.*?thursday|doctors.*?friday|doctors.*?saturday|doctors.*?sunday|\d{1,2}/\d{1,2}/\d{4}.*?bác sĩ|bác sĩ.*?\d{1,2}/\d{1,2}/\d{4})"))
            {
                var date = ParseDateFromMessage(message);
                return await GetDoctorsByDayAsync(date, cancellationToken);
            }
            // 6. Thông tin bác sĩ
            else if (Regex.IsMatch(message, @"(thông tin bác sĩ|chi tiết bác sĩ|doctor info|about doctor|thuộc khoa nào|thông tin chi tiết)"))
            {
                var doctorName = ExtractDoctorName(message);
                if (string.IsNullOrEmpty(doctorName))
                    return "Vui lòng cung cấp tên bác sĩ để xem thông tin.";
                return await GetDoctorInfoAsync(doctorName, cancellationToken);
            }
            // 7. Lịch sử đặt lịch hẹn
            else if (Regex.IsMatch(message, @"(lịch sử đặt lịch|lịch sử hẹn|appointment history|lịch sử đặt khám|lịch hẹn của tôi|đặt lịch khám)"))
            {
                if (!actualUserId.HasValue)
                    return "Vui lòng đăng nhập để xem lịch sử đặt lịch hẹn.";
                return await GetAppointmentHistoryAsync(actualUserId.Value, cancellationToken);
            }
            // 8. Hồ sơ bệnh án
            else if (Regex.IsMatch(message, @"(hồ sơ bệnh án|hồ sơ y tế|medical records)"))
            {
                if (!actualUserId.HasValue)
                    return "Vui lòng đăng nhập để xem hồ sơ bệnh án.";
                return await GetMedicalRecordsAsync(actualUserId.Value, cancellationToken);
            }
            // 9. Số dư ví
            else if (Regex.IsMatch(message, @"(số dư ví|số tiền trong ví|wallet balance)"))
            {
                if (!actualUserId.HasValue)
                    return "Vui lòng đăng nhập để xem số dư ví.";
                return await GetWalletBalanceAsync(actualUserId.Value, cancellationToken);
            }
            // 10. Thông tin hệ thống
            else if (Regex.IsMatch(message, @"(thông tin hệ thống|hệ thống mediappointment|system info|hệ thống)"))
            {
                return GetSystemInfo();
            }
            else
            {
                return "Tôi chưa hiểu câu hỏi của bạn. Vui lòng thử lại với câu hỏi cụ thể hơn, ví dụ: 'Lịch bác sĩ ngày thứ 2', 'Khoa nào có sẵn', hoặc 'Hồ sơ y tế của tôi'.";
            }
        }

        // GetDoctorsByDayAsync: Lấy danh sách bác sĩ làm việc vào một ngày cụ thể
        public async Task<string> GetDoctorsByDayAsync(DateTime date, CancellationToken cancellationToken = default)
        {
            try
            {
                var doctors = await (from roomTimeSlot in _context.RoomTimeSlot
                                     join timeSlot in _context.TimeSlot on roomTimeSlot.TimeSlotId equals timeSlot.Id
                                     join doctor in _context.Set<User>().OfType<Doctor>() on roomTimeSlot.DoctorId equals doctor.Id
                                     join doctorDept in _context.DoctorDepartments on doctor.Id equals doctorDept.DoctorId into deptJoin
                                     from doctorDept in deptJoin.DefaultIfEmpty()
                                     join dept in _context.Departments on doctorDept.DepartmentId equals dept.Id into departmentJoin
                                     from dept in departmentJoin.DefaultIfEmpty()
                                     where roomTimeSlot.Date.Date == date.Date &&
                                           roomTimeSlot.DoctorId != null
                                     select new
                                     {
                                         DoctorId = doctor.Id,
                                         FullName = doctor.FullName,
                                         DepartmentName = dept != null ? dept.DepartmentName : "Không xác định",
                                         HasMorningShift = _context.RoomTimeSlot
                                             .Any(r => r.Date == date &&
                                                      r.DoctorId == doctor.Id &&
                                                      r.TimeSlot.TimeStart >= TimeSpan.FromHours(7) &&
                                                      r.TimeSlot.TimeStart < TimeSpan.FromHours(11)),
                                         HasAfternoonShift = _context.RoomTimeSlot
                                             .Any(r => r.Date == date &&
                                                      r.DoctorId == doctor.Id &&
                                                      r.TimeSlot.TimeStart >= TimeSpan.FromHours(13) &&
                                                      r.TimeSlot.TimeStart < TimeSpan.FromHours(17))
                                     })
                             .Distinct()
                             .ToListAsync(cancellationToken);

                if (!doctors.Any())
                    return $"Không có bác sĩ làm việc vào {date:dd/MM/yyyy}.";

                var response = new StringBuilder();
                response.AppendLine($"Danh sách bác sĩ làm việc vào {date:dd/MM/yyyy}:");

                foreach (var doctor in doctors)
                {
                    var shifts = new List<string>();
                    if (doctor.HasMorningShift) shifts.Add("Sáng: 07:00 - 11:00");
                    if (doctor.HasAfternoonShift) shifts.Add("Chiều: 13:00 - 17:00");

                    response.AppendLine($"- BS. {doctor.FullName} (Khoa {doctor.DepartmentName})");
                    if (shifts.Any())
                        response.AppendLine($"  Lịch làm việc: {string.Join(", ", shifts)}");
                    else
                        response.AppendLine($"  Không có thông tin lịch làm việc");
                }

                return response.ToString();
            }
            catch (Exception ex)
            {
                return $"Đã xảy ra lỗi khi lấy danh sách bác sĩ: {ex.Message}";
            }
        }

        // GetAllDepartmentsAsync: Lấy danh sách tất cả các khoa
        public async Task<string> GetAllDepartmentsAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var departments = await _context.Departments
                    .Select(d => d.DepartmentName)
                    .ToListAsync(cancellationToken);

                if (!departments.Any())
                    return "Hiện tại không có khoa nào trong hệ thống.";

                return $"Danh sách các khoa trong hệ thống: {string.Join(", ", departments)}";
            }
            catch (Exception ex)
            {
                return $"Lỗi khi truy vấn danh sách khoa: {ex.Message}";
            }
        }

        // GetDoctorInfoAsync: Lấy thông tin chi tiết của bác sĩ
        public async Task<string> GetDoctorInfoAsync(string doctorName, CancellationToken cancellationToken = default)
        {
            try
            {
                var doctor = await _context.Doctors
                    .Where(d => d.FullName.ToLower().Contains(doctorName.ToLower()))
                    .Include(d => d.DoctorDepartments)
                        .ThenInclude(dd => dd.Department)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(cancellationToken);

                if (doctor == null)
                    return $"Không tìm thấy thông tin bác sĩ {doctorName}.";

                var departmentNames = doctor.DoctorDepartments != null && doctor.DoctorDepartments.Any()
                    ? string.Join(", ", doctor.DoctorDepartments.Select(dd => dd.Department.DepartmentName))
                    : "Không xác định";

                return $"Bác sĩ {doctor.FullName} thuộc khoa {departmentNames}.";
            }
            catch (Exception ex)
            {
                return $"Lỗi khi truy vấn thông tin bác sĩ: {ex.Message}";
            }
        }

        // GetAllDoctorsAsync: Lấy danh sách tất cả bác sĩ
        private async Task<string> GetAllDoctorsAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var doctors = await _context.Doctors
                    .Where(d => d.Status == Status.Active || d.Status == Status.Inactive)
                    .Include(d => d.DoctorDepartments)
                        .ThenInclude(dd => dd.Department)
                    .AsNoTracking()
                    .Select(d => new
                    {
                        d.FullName,
                        Departments = d.DoctorDepartments != null
                            ? d.DoctorDepartments.Where(dd => dd.Department != null)
                                .Select(dd => dd.Department.DepartmentName)
                                .ToList()
                            : new List<string>()
                    })
                    .ToListAsync(cancellationToken);

                if (!doctors.Any())
                    return "Hiện tại không có bác sĩ nào trong hệ thống.";

                var response = "Danh sách bác sĩ trong hệ thống:\n";
                foreach (var doctor in doctors)
                {
                    var departmentNames = doctor.Departments.Any()
                        ? string.Join(", ", doctor.Departments)
                        : "Không xác định";
                    response += $"- BS. {doctor.FullName} (Khoa: {departmentNames})\n";
                }
                return response;
            }
            catch (Exception ex)
            {
                return $"Lỗi khi truy vấn danh sách bác sĩ: {ex.Message}";
            }
        }

        // GetAppointmentHistoryAsync: Lấy lịch sử đặt lịch hẹn
        public async Task<string> GetAppointmentHistoryAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            try
            {
                var appointments = await _context.Appointments
                    .Where(a => a.PatientId == userId)
                    .Include(a => a.RoomTimeSlot)
                        .ThenInclude(rts => rts.Doctor)
                    .AsNoTracking()
                    .Select(a => new
                    {
                        a.AppointmentDate,
                        DoctorName = a.RoomTimeSlot != null && a.RoomTimeSlot.Doctor != null
                            ? a.RoomTimeSlot.Doctor.FullName
                            : "Không xác định",
                        a.Status,
                        a.Note
                    })
                    .ToListAsync(cancellationToken);

                if (!appointments.Any())
                    return "Bạn chưa có lịch sử đặt khám.";

                var response = $"Lịch sử đặt khám của bạn:\n";
                foreach (var appt in appointments)
                {
                    response += $"- Ngày {appt.AppointmentDate:dd/MM/yyyy HH:mm}, Bác sĩ: {appt.DoctorName}, Trạng thái: {appt.Status}, Ghi chú: {appt.Note ?? "Không có"}\n";
                }
                return response;
            }
            catch (Exception ex)
            {
                return $"Lỗi khi truy vấn lịch sử đặt khám: {ex.Message}";
            }
        }

        // GetAvailableTimeSlotsAsync: Lấy các khung giờ khám bệnh còn trống
        private async Task<string> GetAvailableTimeSlotsAsync(DateTime date, string doctorName, CancellationToken cancellationToken = default)
        {
            try
            {
                var query = _context.RoomTimeSlot
                    .Where(rts => rts.Date.Date == date.Date && rts.Status == RoomTimeSlotStatus.Available)
                    .Include(rts => rts.TimeSlot)
                    .Include(rts => rts.Doctor)
                    .Include(rts => rts.Room)
                        .ThenInclude(r => r.Department)
                    .AsNoTracking();

                if (!string.IsNullOrEmpty(doctorName))
                {
                    query = query.Where(rts => rts.Doctor != null && rts.Doctor.FullName.ToLower().Contains(doctorName.ToLower()));
                }

                var timeSlots = await query
                    .Select(rts => new
                    {
                        TimeStart = rts.TimeSlot != null ? rts.TimeSlot.TimeStart : TimeSpan.Zero,
                        Duration = rts.TimeSlot != null ? rts.TimeSlot.Duration : TimeSpan.Zero,
                        DoctorName = rts.Doctor != null ? rts.Doctor.FullName : "Không xác định",
                        RoomName = rts.Room != null ? rts.Room.Name : "Không xác định",
                        DepartmentName = rts.Room != null && rts.Room.Department != null
                            ? rts.Room.Department.DepartmentName
                            : "Không xác định"
                    })
                    .ToListAsync(cancellationToken);

                if (!timeSlots.Any())
                    return $"Không có khung giờ trống vào ngày {date:dd/MM/yyyy} {(string.IsNullOrEmpty(doctorName) ? "" : $"cho bác sĩ {doctorName}")}.";

                var response = $"Danh sách khung giờ trống vào ngày {date:dd/MM/yyyy}:\n";
                foreach (var slot in timeSlots)
                {
                    response += $"- {slot.TimeStart:hh\\:mm} (Thời lượng: {slot.Duration.TotalMinutes} phút), Bác sĩ: {slot.DoctorName}, Phòng: {slot.RoomName} (Khoa {slot.DepartmentName})\n";
                }
                return response;
            }
            catch (Exception ex)
            {
                return $"Lỗi khi truy vấn khung giờ trống: {ex.Message}";
            }
        }

        // GetMedicalRecordsAsync: Lấy hồ sơ bệnh án
        private async Task<string> GetMedicalRecordsAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            try
            {
                var records = await _context.MedicalRecords
                    .Where(m => m.PatientId == userId)
                    .AsNoTracking()
                    .Select(m => new
                    {
                        m.LastUpdated,
                        m.Diagnosis,
                        m.DoctorName,
                        m.DepartmentVisited
                    })
                    .ToListAsync(cancellationToken);

                if (!records.Any())
                    return "Bạn chưa có hồ sơ y tế.";

                var response = "Hồ sơ y tế của bạn:\n";
                foreach (var record in records)
                {
                    response += $"- Cập nhật: {record.LastUpdated:dd/MM/yyyy}, Chẩn đoán: {record.Diagnosis ?? "Không có"}, Bác sĩ: {record.DoctorName ?? "Không xác định"}, Khoa: {record.DepartmentVisited ?? "Không có"}\n";
                }
                return response;
            }
            catch (Exception ex)
            {
                return $"Lỗi khi truy vấn hồ sơ y tế: {ex.Message}";
            }
        }

        // GetWalletBalanceAsync: Lấy số dư ví
        private async Task<string> GetWalletBalanceAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            try
            {
                var wallet = await _context.Wallets
                    .Where(w => w.UserId == userId)
                    .AsNoTracking()
                    .Select(w => new { w.Balance })
                    .FirstOrDefaultAsync(cancellationToken);

                if (wallet == null)
                    return "Bạn chưa có ví trong hệ thống.";

                return $"Số dư ví của bạn: {wallet.Balance:C}.";
            }
            catch (Exception ex)
            {
                return $"Lỗi khi truy vấn số dư ví: {ex.Message}";
            }
        }

        // GetTransactionHistoryAsync: Lấy lịch sử giao dịch
        private async Task<string> GetTransactionHistoryAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            try
            {
                var transactions = await _context.WalletTransactions
                    .Join(_context.Wallets,
                        wt => wt.WalletId,
                        w => w.Id,
                        (wt, w) => new { WalletTransaction = wt, Wallet = w })
                    .Where(w => w.Wallet.UserId == userId)
                    .AsNoTracking()
                    .Select(w => new
                    {
                        w.WalletTransaction.Amount,
                        w.WalletTransaction.Date,
                        w.WalletTransaction.Type,
                        w.WalletTransaction.Description
                    })
                    .ToListAsync(cancellationToken);

                if (!transactions.Any())
                    return "Bạn chưa có lịch sử giao dịch.";

                var response = "Lịch sử giao dịch của bạn:\n";
                foreach (var transaction in transactions)
                {
                    response += $"- Ngày {transaction.Date:dd/MM/yyyy HH:mm}, Loại: {transaction.Type}, Số tiền: {transaction.Amount:C}, Mô tả: {transaction.Description ?? "Không có"}\n";
                }
                return response;
            }
            catch (Exception ex)
            {
                return $"Lỗi khi truy vấn lịch sử giao dịch: {ex.Message}";
            }
        }

        // GetSystemInfo: Thông tin hệ thống
        private string GetSystemInfo()
        {
            return @"Hệ thống MediAppointment giúp bạn:
- Đặt lịch khám với bác sĩ theo thời gian và khoa phù hợp.
- Xem thông tin bác sĩ, khoa, và lịch làm việc.
- Quản lý hồ sơ y tế và lịch sử đặt khám.
- Theo dõi số dư ví và lịch sử giao dịch.
Hãy hỏi cụ thể để được hỗ trợ, ví dụ: 'Lịch bác sĩ ngày thứ 2', 'Khoa nào có sẵn', hoặc 'Hồ sơ y tế của tôi'.";
        }

        // ParseDateFromMessage: Phân tích ngày từ tin nhắn
        private DateTime ParseDateFromMessage(string message)
        {
            message = message.ToLower().Trim();
            var today = DateTime.Today;

            // Check for specific date (e.g., "25/07/2025")
            var dateMatch = Regex.Match(message, @"(\d{1,2})[/-](\d{1,2})[/-](\d{4})");
            if (dateMatch.Success)
            {
                try
                {
                    var day = int.Parse(dateMatch.Groups[1].Value);
                    var month = int.Parse(dateMatch.Groups[2].Value);
                    var year = int.Parse(dateMatch.Groups[3].Value);
                    return new DateTime(year, month, day);
                }
                catch
                {
                    // Fall through to other checks if parsing fails
                }
            }

            // Check for relative time (today, tomorrow)
            if (message.Contains("hôm nay")) return today;
            if (message.Contains("ngày mai")) return today.AddDays(1);

            // Map days of the week
            var dayMap = new Dictionary<string, DayOfWeek>
    {
        { "thứ hai", DayOfWeek.Monday }, { "thứ 2", DayOfWeek.Monday }, { "monday", DayOfWeek.Monday },
        { "thứ ba", DayOfWeek.Tuesday }, { "thứ 3", DayOfWeek.Tuesday }, { "tuesday", DayOfWeek.Tuesday },
        { "thứ tư", DayOfWeek.Wednesday }, { "thứ 4", DayOfWeek.Wednesday }, { "wednesday", DayOfWeek.Wednesday },
        { "thứ năm", DayOfWeek.Thursday }, { "thứ 5", DayOfWeek.Thursday }, { "thursday", DayOfWeek.Thursday },
        { "thứ sáu", DayOfWeek.Friday }, { "thứ 6", DayOfWeek.Friday }, { "friday", DayOfWeek.Friday },
        { "thứ bảy", DayOfWeek.Saturday }, { "thứ 7", DayOfWeek.Saturday }, { "saturday", DayOfWeek.Saturday },
        { "chủ nhật", DayOfWeek.Sunday }, { "sunday", DayOfWeek.Sunday }
    };

            foreach (var day in dayMap)
            {
                if (message.Contains(day.Key))
                {
                    int daysUntilNext = ((int)day.Value - (int)today.DayOfWeek + 7) % 7;
                    return today.AddDays(daysUntilNext == 0 ? 7 : daysUntilNext); // If same day, return next week
                }
            }

            return today; // Default to today
        }

        // ExtractDoctorName: Trích xuất tên bác sĩ từ tin nhắn
        private string ExtractDoctorName(string message)
        {
            var match = Regex.Match(message, @"bác sĩ\s+([\p{L}\s]+)", RegexOptions.IgnoreCase);
            return match.Success ? match.Groups[1].Value.Trim() : string.Empty;
        }

        // ExtractUserIdFromContext: Trích xuất ID người dùng từ HttpContext
        public Guid? ExtractUserIdFromContext()
        {
            try
            {
                if (_httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated != true)
                {
                    return null;
                }

                var userIdString = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (!string.IsNullOrEmpty(userIdString) && Guid.TryParse(userIdString, out Guid userId))
                {
                    return userId;
                }
                return null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        private async Task<Guid?> GetUserIdFromIdentityId(Guid identityId, CancellationToken cancellationToken)
        {
            try
            {
                var userId = await _context.Set<User>()
                    .Where(u => u.UserIdentityId == identityId)
                    .Select(u => u.Id)
                    .FirstOrDefaultAsync(cancellationToken);
                return userId != default ? userId : null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetUserIdFromIdentityId: {ex.Message}");
                return null;
            }
        }
    }
}