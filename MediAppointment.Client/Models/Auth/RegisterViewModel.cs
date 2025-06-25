using System.ComponentModel.DataAnnotations;

namespace MediAppointment.Client.Models.Auth
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Họ tên là bắt buộc")]
        [Display(Name = "Họ và tên")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Giới tính là bắt buộc")]
        [Display(Name = "Giới tính")]
        public bool Gender { get; set; }

        [Required(ErrorMessage = "Ngày sinh là bắt buộc")]
        [DataType(DataType.Date)]
        [Display(Name = "Ngày sinh")]
        public DateTime DateOfBirth { get; set; }

        [Required(ErrorMessage = "Email là bắt buộc")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Số điện thoại là bắt buộc")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        [Display(Name = "Số điện thoại")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mật khẩu là bắt buộc")]
        [DataType(DataType.Password)]
        [MinLength(6, ErrorMessage = "Mật khẩu phải có ít nhất 6 ký tự")]
        [Display(Name = "Mật khẩu")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Xác nhận mật khẩu là bắt buộc")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Mật khẩu xác nhận không khớp")]
        [Display(Name = "Xác nhận mật khẩu")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vai trò là bắt buộc")]
        [Display(Name = "Đăng ký là")]
        public string Role { get; set; } = "Patient";

        // Additional properties for Patient
        [Display(Name = "CCCD")]
        public string? CCCD { get; set; }

        [Display(Name = "Địa chỉ")]
        public string? Address { get; set; }

        [Display(Name = "Số BHYT")]
        public string? BHYT { get; set; }
    }
}
