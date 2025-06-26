using MediAppointment.Application.DTOs;
using MediAppointment.Application.DTOs.Auth;
using MediAppointment.Application.DTOs.DoctorDTOs;
using MediAppointment.Application.DTOs.Pages;
using MediAppointment.Domain.Entities;

namespace MediAppointment.Application.Interfaces
{
    public interface IIdentityService
    {
        #region Manager
        Task<PagedResult<DoctorDto>> GetAllDoctorsAsync(string text = "", string department = "", int page = 1, int pageSize = 5);
        Task<Guid> CreateDoctorAsync(DoctorCreateDto dto);
        Task ManagerUpdateDoctorAsync(ManagerDoctorUpdateDTO dto);
        Task DeleteDoctorAsync(Guid doctorId);
        #endregion

        #region DoctorProfile
        Task<DoctorDto> GetDoctorByIdAsync(Guid doctorId);
        Task UpdateDoctorAsync(Guid userIdentityId, DoctorUpdateDto dto);
        #endregion

        #region PatientProfile
        Task<Guid> CreatePatientAsync(PatientCreateDto dto);
        Task UpdatePatientAsync(PatientUpdateDto dto);
        Task DeletePatientAsync(Guid patientId);
        Task<PatientUpdateDto?> GetPatientByIdAsync(Guid patientId);
        #endregion

        #region Login
        Task<LoginResultDto> LoginAsync(LoginDto dto);
        #endregion

        #region Register
        Task<LoginResultDto> RegisterAsync(RegisterDto dto);
        Task<LoginResultDto> RefreshTokenAsync(RefreshTokenDto dto);
        Task<LoginResultDto> ConfirmEmailAsync(string email, string token);
        #endregion

        #region ForgotPass
        Task<bool> ForgotPasswordAsync(ForgotPasswordDto dto);
        Task<LoginResultDto> ResetPasswordAsync(ResetPasswordDto dto);
        #endregion

        #region Logout
        Task LogoutAsync();
        #endregion

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