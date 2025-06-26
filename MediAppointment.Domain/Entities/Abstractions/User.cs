using MediAppointment.Domain.Enums;

namespace MediAppointment.Domain.Entities.Abstractions
{
    public abstract class User : Entity
    {
        public string FullName { get; set; } = default!;
        public bool Gender { get; set; } = default!;
        public DateTime DateOfBirth { get; set; }
        public string Email { get; set; } = default!;
        public string PhoneNumber { get; set; } = default!;
        public Status Status { get; set; } = Status.Active;
    }
}


