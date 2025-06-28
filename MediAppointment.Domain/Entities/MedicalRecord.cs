using MediAppointment.Domain.Entities.Abstractions;

namespace MediAppointment.Domain.Entities
{
    public class MedicalRecord : Entity
    {
        public Guid? DoctorId { get; set; }
        public Guid PatientId { get; set; }
        public float? Height { get; set; }
        public float? Weight { get; set; }
        public string? BloodType { get; set; }               // Nhóm máu
        public string? Chronic { get; set; }                 // Bệnh mãn tính
        public string? MedicalHistory { get; set; }          // Tiền sử bệnh
        public string? MedicalResult { get; set; }           // Kết quả khám
        public string? LastUpdated { get; set; }             // Ngày cập nhật (có thể đổi thành DateTime nếu cần)



        // Các trường bổ sung
        public string? Diagnosis { get; set; }               // Chẩn đoán
        public string? TreatmentPlan { get; set; }           // Phác đồ điều trị
        public string? Allergies { get; set; }               // Dị ứng
        public string? Medications { get; set; }             // Thuốc đang dùng
        public string? Symptoms { get; set; }                // Triệu chứng
        public string? VitalSigns { get; set; }              // Dấu hiệu sinh tồn (huyết áp, nhịp tim, SpO2)
        public DateTime? NextAppointmentDate { get; set; }   // Lịch khám tiếp theo
        public string? DepartmentVisited { get; set; }       // Khoa khám
        public string? DoctorName { get; set; }              // Tên bác sĩ phụ trách (nếu không lấy từ bảng Doctor)

    }
}
