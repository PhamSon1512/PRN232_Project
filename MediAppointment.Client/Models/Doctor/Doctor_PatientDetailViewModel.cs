using MediAppointment.Client.Models.MedicalRecord;

namespace MediAppointment.Client.Models.Doctor
{
    public class Doctor_PatientDetailViewModel
    {
        public Guid Id { get; set; }
        public string FullName { get; set; } = "";
        public bool Gender { get; set; }  
        public DateTime DateOfBirth { get; set; }
        public string Email { get; set; } = "";
        public string PhoneNumber { get; set; } = "";
        public string Cccd { get; set; } = "";
        public string Address { get; set; } = "";
        public string Bhyt { get; set; } = "";
        public List<MedicalRecordViewModel> MedicalRecords { get; set; } = new();
    }
}
