using MediAppointment.Domain.Entities;
using MediAppointment.Domain.Enums;
using Microsoft.AspNetCore.Identity;

namespace MediAppointment.Infrastructure.Identity
{
    public class UserIdentity : IdentityUser<Guid> 
    {
        public string? FullName  { get; set; }
        public string RefreshToken { get; internal set; }
        public DateTime RefreshTokenExpiryTime { get; internal set; }
        public DateTime DateOfBirth { get; set; }
        public bool Gender { get; set; }
        public Status Status { get; set; }
    }

}
