namespace MediAppointment.Client.Models.MedicalRecord
{
    public class CreateMedicalRecordRequest
    {
        public Guid PatientId { get; set; }
        public int Height { get; set; }
        public int Weight { get; set; }
        public string? BloodType { get; set; }
        public string? Chronic { get; set; }
        public string? MedicalHistory { get; set; }
        public string? MedicalResult { get; set; }
        public string? Allergies { get; set; }
        public string? DepartmentVisited { get; set; }
        public string? Diagnosis { get; set; }
        public string? DoctorName { get; set; }
        public string? Medications { get; set; }
        public DateTime? NextAppointmentDate { get; set; }
        public string? Symptoms { get; set; }
        public string? TreatmentPlan { get; set; }
        public string? VitalSigns { get; set; }
    }
}
