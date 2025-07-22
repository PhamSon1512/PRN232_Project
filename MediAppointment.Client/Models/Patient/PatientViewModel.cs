using MediAppointment.Client.Models.MedicalRecord;

namespace MediAppointment.Client.Models.Patient
{
    public class PatientViewModel
    {
        public Guid Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public bool Gender { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string? CCCD { get; set; }
        public string? Address { get; set; }
        public string? BHYT { get; set; }
        public int Age { get; set; }
        public string GenderDisplay => Gender ? "Nam" : "Nữ";
        public List<MedicalRecordViewModel>? MedicalRecords { get; set; } = new();

    }
}
