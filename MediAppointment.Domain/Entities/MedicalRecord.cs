namespace MediAppointment.Domain.Entities
{
    public class MedicalRecord
    {
        public int RecordID { get; set; }
        public int DoctorID { get; set; }
        public int UserID { get; set; }
        public float Height { get; set; }
        public float Weight { get; set; }
        public string? BloodType { get; set; }
        public string? Chronic { get; set; }
        public string? MedicalHistory { get; set; }     
        public string? MedicalResult { get; set; }
        public string? LastUpdated { get; set; }

        public Doctor? Doctor { get; set; }
    }
}
