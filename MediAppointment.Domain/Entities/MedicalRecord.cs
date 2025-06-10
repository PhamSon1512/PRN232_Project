using MediAppointment.Domain.Entities.Abstractions;

namespace MediAppointment.Domain.Entities
{
    public class MedicalRecord : Entity
    {
        public Guid DoctorId { get; set; }
        public Guid PatientId { get; set; }
        public float Height { get; set; }
        public float Weight { get; set; }
        public string? BloodType { get; set; }
        public string? Chronic { get; set; }
        public string? MedicalHistory { get; set; }     
        public string? MedicalResult { get; set; }
        public string? LastUpdated { get; set; }
    }
}
