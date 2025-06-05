namespace MediAppointment.Domain.Entities
{
    public class User
    {
        public int Id { get; set; }
        public string? FullName { get; set; }
        public int Gender { get; set; } // 0: Female, 1: Male
        public DateTime DOB { get; set; }
        public int Age { get; set; }
        public int Status { get; set; }
        public string? Address { get; set; }
        public DateTime CreateDate { get; set; }
        public string? CCCD { get; set; } // 12 digits
    }
}
