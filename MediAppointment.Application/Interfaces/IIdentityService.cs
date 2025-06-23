using MediAppointment.Application.DTOs;
using MediAppointment.Application.DTOs.Auth;

namespace MediAppointment.Application.Interfaces
{
    public interface IIdentityService
    {
        // Doctor
        Task<Guid> CreateDoctorAsync(DoctorCreateDto dto);
        Task UpdateDoctorAsync(Guid userIdentityId, DoctorUpdateDto dto);
        Task DeleteDoctorAsync(Guid doctorId);
        //Task<DoctorUpdateDto?> GetDoctorByIdAsync(Guid doctorId);

        // Patient
        Task<Guid> CreatePatientAsync(PatientCreateDto dto);
        Task UpdatePatientAsync(PatientUpdateDto dto);
        Task DeletePatientAsync(Guid patientId);
        Task<PatientUpdateDto?> GetPatientByIdAsync(Guid patientId);

        // Login
        Task<LoginResultDto> LoginAsync(LoginDto dto);

        // Register
        Task<LoginResultDto> RegisterAsync(RegisterDto dto);
        //Forgot Password
        Task<bool> ForgotPasswordAsync(ForgotPasswordDto dto);
        //Logout
        Task LogoutAsync();
    }
}
