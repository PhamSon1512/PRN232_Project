namespace MediAppointment.Domain.Entities
{
    public class Department
    {
        public int DepartmentID { get; set; }
        public string? DepartmentName { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedTime { get; set; }
        public int? UpdatedPersonId { get; set; }
    }
}
