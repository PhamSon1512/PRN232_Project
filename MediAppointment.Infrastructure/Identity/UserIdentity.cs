using MediAppointment.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace MediAppointment.Infrastructure.Identity
{
    public class UserIdentity : IdentityUser<int>
    {
        public string? FullName { get; set; }
        public int Gender { get; set; }
        public DateTime DOB { get; set; }
        public int Age { get; set; }
        public int Status { get; set; }
        public string? Address { get; set; }
        public DateTime CreateDate { get; set; }
        public string? CCCD { get; set; }
        public ICollection<Doctor>? Doctors { get; set; }
        public ICollection<MedicalRecord>? MedicalRecords { get; set; }
        public ICollection<Appointment>? Appointments { get; set; }
        public ICollection<Notification>? Notifications { get; set; }
    }

}
