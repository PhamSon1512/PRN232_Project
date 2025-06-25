using MediAppointment.Application.DTOs;
using MediAppointment.Application.DTOs.Auth;

namespace MediAppointment.Application.Interfaces
{
    public interface IIdentityService
    {
        // Doctor
        Task<Guid> CreateDoctorAsync(DoctorCreateDto dto);
        Task UpdateDoctorAsync(DoctorUpdateDto dto);
        Task DeleteDoctorAsync(Guid doctorId);
        Task<DoctorUpdateDto?> GetDoctorByIdAsync(Guid doctorId);

        // Patient
        Task<Guid> CreatePatientAsync(PatientCreateDto dto);
        Task UpdatePatientAsync(PatientUpdateDto dto);
        Task DeletePatientAsync(Guid patientId);
        Task<PatientUpdateDto?> GetPatientByIdAsync(Guid patientId);

        // Login
        Task<LoginResultDto> LoginAsync(LoginDto dto);

        // Register
        Task<LoginResultDto> RegisterAsync(RegisterDto dto);

        Task<LoginResultDto> ConfirmEmailAsync(string email, string token);

        //Forgot Password
        Task<bool> ForgotPasswordAsync(ForgotPasswordDto dto);
        Task<LoginResultDto> ResetPasswordAsync(ResetPasswordDto dto);

        //Logout
        Task LogoutAsync();


        //Task<bool> CreateRoleAsync(string roleName);
        //Task<bool> DeleteRoleAsync(string roleId);
        //Task<List<(Guid id, string roleName)>> GetRolesAsync();
        //Task<(Guid id, string roleName)> GetRoleByIdAsync(string id);
        //Task<bool> UpdateRole(Guid id, string roleName);

        //Task<bool> IsInRoleAsync(string userId, string role);
        //Task<List<string>> GetUserRolesAsync(string userId);
        //Task<bool> AssignUserToRole(string userName, IList<string> roles);

        //Task<bool> UpdateUsersRole(string userName, IList<string> usersRole);
        //Task<(bool isUserExists, bool isConfirmed)> CheckUserExistsWithEmailConfirmedAsync(string email);
        //Task<string> GenerateEmailConfirmationTokenAsync(string email);
    }
}
