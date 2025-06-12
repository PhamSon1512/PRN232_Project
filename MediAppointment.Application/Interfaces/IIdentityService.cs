using MediAppointment.Application.DTOs;

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
    }
}
