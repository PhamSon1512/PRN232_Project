using MediAppointment.Domain.Entities.Abstractions;

namespace MediAppointment.Domain.Entities
{
    public class MedicalRecord : Entity
    {
        public Guid PatientId { get; set; }     // FK tới ApplicationUser (Patient)
        public Guid? DoctorId { get; set; }     // FK tới ApplicationUser (Doctor)

        // Thông tin y tế
        public float? Height { get; set; }
        public float? Weight { get; set; }
        public string? BloodType { get; set; }
        public string? Chronic { get; set; }
        public string? MedicalHistory { get; set; }
        public string? MedicalResult { get; set; }

        public DateTime? LastUpdated { get; set; }

        public string? Diagnosis { get; set; }
        public string? TreatmentPlan { get; set; }
        public string? Allergies { get; set; }
        public string? Medications { get; set; }
        public string? Symptoms { get; set; }
        public string? VitalSigns { get; set; }
        public DateTime? NextAppointmentDate { get; set; }
        public string? DepartmentVisited { get; set; }

        public string? DoctorName { get; set; }

       
    }
}
