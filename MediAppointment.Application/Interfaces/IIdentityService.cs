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
        #endregion

        #region Register
        Task<bool> ForgotPasswordAsync(ForgotPasswordDto dto);
        #endregion

        #region Register
        Task LogoutAsync();
        #endregion
    }
}
