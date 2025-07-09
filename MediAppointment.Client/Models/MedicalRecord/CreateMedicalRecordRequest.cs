namespace MediAppointment.Client.Models.MedicalRecord
{
    using System;
    using System.ComponentModel.DataAnnotations;

    public class CreateMedicalRecordRequest
    {
        [Required]
        public Guid PatientId { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập chiều cao")]
        [Range(1, 300, ErrorMessage = "Chiều cao phải từ 1 đến 300 cm")]
        public int? Height { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập cân nặng")]
        [Range(1, 500, ErrorMessage = "Cân nặng phải từ 1 đến 500 kg")]
        public int? Weight { get; set; }

        [StringLength(5, ErrorMessage = "Nhóm máu không hợp lệ")]
        public string? BloodType { get; set; }

        [StringLength(1000)]
        public string? Chronic { get; set; }

        [StringLength(2000)]
        public string? MedicalHistory { get; set; }

        [StringLength(2000)]
        public string? MedicalResult { get; set; }

        [StringLength(1000)]
        public string? Allergies { get; set; }

        [StringLength(255)]
        public string? DepartmentVisited { get; set; }

        [StringLength(2000)]
        public string? Diagnosis { get; set; }

        [StringLength(255)]
        public string? DoctorName { get; set; }

        [StringLength(2000)]
        public string? Medications { get; set; }

        [DataType(DataType.Date)]
        public DateTime? NextAppointmentDate { get; set; }

        [StringLength(2000)]
        public string? Symptoms { get; set; }

        [StringLength(2000)]
        public string? TreatmentPlan { get; set; }

        [StringLength(255)]
        public string? VitalSigns { get; set; }
    }

}
