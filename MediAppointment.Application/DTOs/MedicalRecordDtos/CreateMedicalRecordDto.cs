using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediAppointment.Application.DTOs.MedicalRecordDtos
{
    public class CreateMedicalRecordDto
    {
        public Guid PatientId { get; set; }
        public Guid? DoctorId { get; set; } // Có thể null nếu bệnh án từ bên ngoài

        public float? Height { get; set; }
        public float? Weight { get; set; }
        public string? BloodType { get; set; }

        public string? Chronic { get; set; }
        public string? MedicalHistory { get; set; }
        public string? MedicalResult { get; set; }

        public string? Allergies { get; set; }
        public string? DepartmentVisited { get; set; }
        public string? Diagnosis { get; set; }
        public string? DoctorName { get; set; } // Tên bác sĩ tạo hồ sơ nếu không có ID

        public string? Medications { get; set; }
        public DateTime? NextAppointmentDate { get; set; }
        public string? Symptoms { get; set; }
        public string? TreatmentPlan { get; set; }
        public string? VitalSigns { get; set; }
    }
}
