using MediAppointment.Application.DTOs.PatientDTOs;


namespace MediAppointment.Application.Interfaces
{
    public interface IPatientService
    {
    Task<PatientWithRecordsResponse?> GetPatientWithRecordsAsync(Guid patientId);
    }
}
