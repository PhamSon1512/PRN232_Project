using System.ComponentModel.DataAnnotations;

namespace MediAppointment.Client.Models.Admin
{
    public class AdminViewModel
    {
        public Guid Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public DateTime DateOfBirth { get; set; }
        public bool Gender { get; set; }
        public string PhoneNumber { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public string ErrorMessage { get; set; }
    }

    public class EditUserViewModel
    {
        [Required(ErrorMessage = "Họ và tên không được để trống.")]
        [RegularExpression(@"^[a-zA-Z\s]+$", ErrorMessage = "Họ và tên không được chứa số và ký tự đặc biệt.")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Số điện thoại không được để trống.")]
        [RegularExpression(@"^\d+$", ErrorMessage = "Số điện thoại chỉ được chứa số.")]
        [StringLength(11, MinimumLength = 10, ErrorMessage = "Số điện thoại phải có đúng 10 chữ số.")]
        public string PhoneNumber { get; set; }

        public string Role { get; set; }
        public bool IsActive { get; set; }
    }

    public class AdminEditProfileModel
    {
        public Guid Id { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
    }

    public class AdminUpdateProfile
    {
        public Guid? Id { get; set; }

        [Required(ErrorMessage = "Họ và tên không được để trống.")]
        [RegularExpression(@"^[a-zA-ZÀ-ỹ\s]+$", ErrorMessage = "Họ và tên chỉ được chứa chữ cái và khoảng trắng.")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Số điện thoại không được để trống.")]
        [RegularExpression(@"^\d+$", ErrorMessage = "Số điện thoại chỉ được chứa số.")]
        [StringLength(10, ErrorMessage = "Số điện thoại phải có tối đa 10 chữ số.")]
        public string PhoneNumber { get; set; }
    }

    public class DashboardViewModel
    {
        public int TotalDoctors { get; set; }
        public int TotalManagers { get; set; }
        public int TotalPatients { get; set; }
        public int TotalAppointments { get; set; }
        public int PendingAppointments { get; set; }
        public int CompletedAppointments { get; set; }
        public int CancelledAppointments { get; set; }
        public decimal TotalRevenue { get; set; }
    }
}